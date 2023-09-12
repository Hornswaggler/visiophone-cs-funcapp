using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CloudConvert.API;
using CloudConvert.API.Models.ExportOperations;
using CloudConvert.API.Models.ImportOperations;
using CloudConvert.API.Models.JobModels;
using CloudConvert.API.Models.TaskOperations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using vp.models;
using vp.services;
using vp.util;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackActivities
    {
        private static IStorageService _storageService;
        private static ISamplePackService _samplePackService;

        public UpsertSamplePackActivities(ISamplePackService samplePackService, IStorageService storageService)
        {
            _samplePackService = samplePackService;
            _storageService = storageService;
        }

        [FunctionName(ActivityNames.UpsertSamplePackMetadata)]
        public async Task<SamplePack<Sample>> UpsertSamplePackMetadata(
            [ActivityTrigger] SamplePack<Sample> samplePack)
        {
            var result = await _samplePackService.AddSamplePack(samplePack);
            return result;
        }

        [FunctionName(ActivityNames.UpsertSamplePackTransferImage)]
        public async Task<UpsertSamplePackTransaction> UpsertSamplePackTransferImage(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
        )
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient destContainer = _blobServiceClient.GetBlobContainerClient(Config.CoverArtContainerName);
            var destBlob = destContainer.GetBlobClient(upsertSamplePackTransaction.request.imgUrl);
            
            await destBlob.StartCopyFromUriAsync(BlobFactory.GetBlobSasToken(Config.UploadStagingContainerName, upsertSamplePackTransaction.request.imgUrl));

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.CleanupStagingData)]
        public async Task<UpsertSamplePackTransaction> CleanupStagingData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
)
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient srcContainer = _blobServiceClient.GetBlobContainerClient(Config.UploadStagingContainerName);
            var imgBlob = srcContainer.GetBlobClient(upsertSamplePackTransaction.request.imgUrl);

            await imgBlob.DeleteIfExistsAsync();

            foreach(var sampleRequest in upsertSamplePackTransaction.request.samples)
            {
                var sampleBlob = srcContainer.GetBlobClient(sampleRequest.clipUri);
                await sampleBlob.DeleteIfExistsAsync();
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.InitiateCloudConvertJob)]
        public async Task<UpsertSamplePackTransaction> InitiateCloudConvertJob(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction, ILogger log)
        {

            //TODO: Move credentials stuff to "Util"
            //var mediaServicesResourceId = MediaServicesAccountResource.CreateResourceIdentifier(
            //    subscriptionId: Config.StorageSubscriptionId,
            //    resourceGroupName: Config.StorageResourceGroupName,
            //    accountName: Config.SampleTranscodeContainerName
            //);

            //var credential = new DefaultAzureCredential();
            //var armClient = new ArmClient(credential);
            //var mediaServicesAccount = armClient.GetMediaServicesAccountResource(mediaServicesResourceId);

            //MediaAssetResource asset;


            //foreach (var sample in upsertSamplePackTransaction.request.samples)
            //{
            //    string importSampleFileName = $"{sample.id}.wav";
            //    string exportSampleFileName = $"{sample.id}.mp3";

                //try
                //{
                //    //TODO: Hardcoded file type...
                //    asset = await mediaServicesAccount.GetMediaAssets().GetAsync(exportSampleFileName);

                //    // The Asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
                //    // an existing Asset, use an unique name.
                //    Console.WriteLine($"Warning: The Asset named {exportSampleFileName} already exists. It will be overwritten.");
                //}
                //catch (RequestFailedException)
                //{
                //    // Call Media Services API to create an Asset.
                //    // This method creates a container in storage for the Asset.
                //    // The files (blobs) associated with the Asset will be stored in this container.
                //    Console.WriteLine("Creating an input Asset...");
                //    asset = (await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(WaitUntil.Completed, exportSampleFileName, new MediaAssetData())).Value;
                //}

                ////CLOUD CONVERT TEST CODE...
                var cloudConvert = new CloudConvertAPI(Config.CloudConvertAPIKey);


            //var sasUriCollection = asset.GetStorageContainerUrisAsync(
            //    new MediaAssetStorageContainerSasContent
            //    {
            //        Permissions = MediaAssetContainerPermission.Read,
            //        ExpireOn = DateTime.UtcNow.AddHours(1)
            //    }).GetAsyncEnumerator();
            //await sasUriCollection.MoveNextAsync();
            //var exportSasToken = sasUriCollection.Current;








            //var importSasToken = _storageService.GetSASTokenForUploadBlob(importSampleFileName, BlobSasPermissions.Read);
            //var exportSasToken = _storageService.GetSASTokenForTranscodeBlob(exportSampleFileName, BlobSasPermissions.Read | BlobSasPermissions.Write);

            //TODO: do not await... let callbacks handle this
            try
            {
                dynamic taskDefinitions = new Dictionary<string, object>();
                foreach (var sample in upsertSamplePackTransaction.request.samples)
                {
                    string importSampleFileName = $"{upsertSamplePackTransaction.request.id}/{sample.id}.wav";
                    string exportSampleFileName = $"{sample.id}.mp3";

                    //TODO: ... UGH
                    var importSasToken = $"?{_storageService.GetSASTokenForUploadBlob(importSampleFileName, BlobSasPermissions.Read).ToString().Split('?').Last()}";
                    var exportSasToken = $"?{_storageService.GetSASTokenForTranscodeBlob(exportSampleFileName, BlobSasPermissions.Read | BlobSasPermissions.Write).ToString().Split('?').Last()}";

                    string importTaskName = $"import_{sample.id}";
                    string convertTaskName = $"convert_{sample.id}";
                    string exportTaskName = $"export_{sample.id}";

                    taskDefinitions[importTaskName] = new ImportAzureBlobCreateRequest
                    {
                        Storage_Account = Config.StorageAccountName,
                        Container = Config.SampleBlobContainerName,
                        Sas_Token = importSasToken,
                        Blob = importSampleFileName
                    };

                    taskDefinitions[convertTaskName] = new ConvertCreateRequest
                    {
                        Input = importTaskName,
                        Input_Format = "wav",
                        Output_Format = "mp3",
                        Engine = "ffmpeg",
                        Options = new Dictionary<string, object>
                        {
                            ["audio_codec"] = "mp3",
                            ["audio_qscale"] = 0
                        }
                    };

                    taskDefinitions[$"export_{sample.id}"] = new ExportAzureBlobCreateRequest
                    {
                        Input = convertTaskName,
                        Storage_Account = Config.StorageAccountName,
                        Container = Config.SampleTranscodeContainerName,
                        Sas_Token = exportSasToken.ToString(),
                        Blob = exportSampleFileName
                    };
                }

                var job = await cloudConvert.CreateJobAsync(new JobCreateRequest
                {
                    Tasks = taskDefinitions
                });

            }catch(Exception e)
            {
                log.LogError($"Failed to initiate cloud convert: {e.Message}", e);
            }
            //}

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.UpsertStripeData)]

        public static async Task<UpsertSamplePackTransaction> UpsertStripeData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSampleTransaction,
            ILogger log)
        {
            var sampleMetadata = upsertSampleTransaction.request;
            var account = upsertSampleTransaction.account;

            var sampleDescriptions = sampleMetadata.samples.Aggregate("", (acc, sample) =>
            {
                return $"{(acc == "" ? "" : $"{acc}, ")}{sample.name}";
            });

            var options = new Stripe.ProductCreateOptions
            {
                //TODO: Magic number
                Name = sampleMetadata.name,
                Description = $"{sampleMetadata.description}: {sampleDescriptions}",

                //Default to the currency of the User Account (Probably is affiliated w/ the account?)
                DefaultPriceData = new Stripe.ProductDefaultPriceDataOptions
                {
                    Currency = account.defaultCurrency,
                    UnitAmountDecimal = sampleMetadata.cost
                },
                Metadata = new Dictionary<string, string>
                {
                    { "accountId", $"{account.stripeId}" },
                    { "id", $"{upsertSampleTransaction.request.id}" },
                    { "type", "samplePacks"}
                }
            };

            var service = new Stripe.ProductService();
            var stripeProduct = await service.CreateAsync(options);
            sampleMetadata.priceId = stripeProduct.DefaultPriceId;
            sampleMetadata.sellerId = account.accountId;

            upsertSampleTransaction.request = sampleMetadata;

            return upsertSampleTransaction;
        }
    }
}
