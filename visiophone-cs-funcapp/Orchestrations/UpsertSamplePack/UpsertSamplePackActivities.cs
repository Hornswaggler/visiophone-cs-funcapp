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

        [FunctionName(ActivityNames.MigrateSamplePackAssets)]
        public async Task<UpsertSamplePackTransaction> MigrateSamplePackAssets(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
        )
        {

            throw new Exception("MigrateSamplePackAssetsFailed");

            var samplePackRequest = upsertSamplePackTransaction.request;
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient previewContainer = _blobServiceClient.GetBlobContainerClient(Config.SamplePreviewContainerName);
            BlobContainerClient sampleContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleFilesContainerName);

            //Migrate Samples to preview and high quality containers
            foreach (var sampleRequest in samplePackRequest.samples)
            {
                var samplePreviewBlob = previewContainer.GetBlobClient($"{sampleRequest.previewBlobName}");
                await samplePreviewBlob.StartCopyFromUriAsync(BlobFactory.GetBlobSasToken(Config.UploadStagingContainerName, sampleRequest.exportBlobName));
                
                var sampleBlob = sampleContainer.GetBlobClient($"{sampleRequest.sampleBlobName}");
                await sampleBlob.StartCopyFromUriAsync(BlobFactory.GetBlobSasToken(Config.UploadStagingContainerName, sampleRequest.importBlobName));
            }

            //Migrate image to preview container
            var imagePreviewBlob = previewContainer.GetBlobClient($"{samplePackRequest.imgBlobName}");
            await imagePreviewBlob.StartCopyFromUriAsync(BlobFactory.GetBlobSasToken(Config.UploadStagingContainerName, samplePackRequest.exportImgBlobName));

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.CleanupStagingData)]
        public async Task<UpsertSamplePackTransaction> CleanupStagingData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
)
        {
            //TODO: implement this way: await blobClient.DeleteBlobIfExistsAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
            var samplePackRequest = upsertSamplePackTransaction.request;

            BlobContainerClient srcContainer = _blobServiceClient.GetBlobContainerClient(Config.UploadStagingContainerName);
            var importImageBlob = srcContainer.GetBlobClient(samplePackRequest.importImgBlobName);
            await importImageBlob.DeleteIfExistsAsync();

            var exportImageBlob = srcContainer.GetBlobClient(samplePackRequest.exportImgBlobName);
            await exportImageBlob.DeleteIfExistsAsync();

            foreach (var sampleRequest in samplePackRequest.samples)
            {
                var importSampleBlob = srcContainer.GetBlobClient(sampleRequest.importBlobName);
                await importSampleBlob.DeleteIfExistsAsync();

                var exportSampleBlob = srcContainer.GetBlobClient(sampleRequest.exportBlobName);
                await exportSampleBlob.DeleteIfExistsAsync();
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.ConvertSamplePackAssets)]
        public async Task<UpsertSamplePackTransaction> ConvertSamplePackAssets(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction, ILogger log)
        {
            var cloudConvert = new CloudConvertAPI(Config.CloudConvertAPIKey);
            var samplePackRequest = upsertSamplePackTransaction.request;

            try
            {
                dynamic taskDefinitions = new Dictionary<string, object>();

                throw new Exception("OOF!");

                //Sample Conversion(s)
                foreach (var sample in samplePackRequest.samples)
                {
                    //TODO: ... UGH
                    var importSasToken = $"?{_storageService.GetSASTokenForUploadBlob(sample.importBlobName, BlobSasPermissions.Read).ToString().Split('?').Last()}";
                    var exportSasToken = $"?{_storageService.GetSASTokenForUploadBlob(sample.exportBlobName, BlobSasPermissions.Read | BlobSasPermissions.Write).ToString().Split('?').Last()}";

                    string importTaskName = $"import_{sample.id}";
                    string convertTaskName = $"convert_{sample.id}";
                    string exportTaskName = $"export_{sample.id}";

                    taskDefinitions[importTaskName] = new ImportAzureBlobCreateRequest
                    {
                        Storage_Account = Config.StorageAccountName,
                        Container = Config.SampleBlobContainerName,
                        Sas_Token = importSasToken,
                        Blob = sample.importBlobName
                    };

                    taskDefinitions[convertTaskName] = new ConvertCreateRequest
                    {
                        Input = importTaskName,
                        Input_Format = $"{sample.fileExtension}",
                        Output_Format = $"{Config.ClipExportFileFormat}",
                        Engine = "ffmpeg",
                        Options = new Dictionary<string, object>
                        {
                            ["audio_codec"] = $"{Config.ClipExportFileFormat}",
                            ["audio_qscale"] = 0
                        }
                    };

                    taskDefinitions[$"export_{sample.id}"] = new ExportAzureBlobCreateRequest
                    {
                        Input = convertTaskName,
                        Storage_Account = Config.StorageAccountName,
                        Container = Config.SampleBlobContainerName,
                        Sas_Token = exportSasToken,
                        Blob = sample.exportBlobName
                    };
                }

                var importImageSasToken = $"?{_storageService.GetSASTokenForUploadBlob(samplePackRequest.importImgBlobName, BlobSasPermissions.Read).ToString().Split('?').Last()}";
                var exportImageSasToken = $"?{_storageService.GetSASTokenForUploadBlob(samplePackRequest.exportImgBlobName, BlobSasPermissions.Read | BlobSasPermissions.Write).ToString().Split('?').Last()}";

                var importImageTaskName = "import_sample_pack_image";
                var convertImageTaskName = "convert_sample_pack_image";
                var exportImageTaskName = "export_sample-pack_image";

                //Sample Pack Thumbnail conversion
                taskDefinitions[importImageTaskName] = new ImportAzureBlobCreateRequest
                {
                    Storage_Account = Config.StorageAccountName,
                    Container = Config.SampleBlobContainerName,
                    Sas_Token = importImageSasToken,
                    Blob = samplePackRequest.importImgBlobName
                };

                taskDefinitions[convertImageTaskName] = new ConvertCreateRequest
                {
                    Input = importImageTaskName,
                    Input_Format = $"{samplePackRequest.imgUrlExtension}",
                    Output_Format = $"{Config.ImageExportFileFormat}",
                    Options = new Dictionary<string, object>
                    {
                        ["fit"] = "max",
                        ["strip"] = true,
                        ["quality"] = Config.ImageExportQuality,
                        ["auto_orient"] = true,
                        ["width"] = Config.ImageExportWidth,
                        ["height"] = Config.ImageExportHeight
                    }
                };

                taskDefinitions[exportImageTaskName] = new ExportAzureBlobCreateRequest
                {
                    Input = convertImageTaskName,
                    Storage_Account = Config.StorageAccountName,
                    Container = Config.SampleBlobContainerName,
                    Sas_Token = exportImageSasToken,
                    Blob = samplePackRequest.exportImgBlobName
                };

                //TODO: do not await... let callbacks handle this
                var job = await cloudConvert.CreateJobAsync(new JobCreateRequest
                {
                    Tasks = taskDefinitions
                });
            }
            catch(Exception e)
            {
                log.LogError($"Failed to initiate cloud convert: {e.Message}", e);
                throw new Exception("OOF!");
            }

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
