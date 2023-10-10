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
            await _storageService.MigrateUploadsForSamplePackTransaction(upsertSamplePackTransaction);
            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.CleanupStagingData)]
        public async Task<UpsertSamplePackTransaction> CleanupStagingData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            await _storageService.DeleteUploadsForSamplePackTransaction(upsertSamplePackTransaction);
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


                //Sample Conversion(s)
                foreach (var sample in samplePackRequest.samples)
                {
                    var importSasToken = _storageService.GetSASTokenForUploadStagingBlob(sample.importBlobName, BlobSasPermissions.Read);
                    var exportSasToken = _storageService.GetSASTokenForUploadStagingBlob(sample.exportBlobName, BlobSasPermissions.Read | BlobSasPermissions.Write);

                    string importTaskName = $"import_{sample.id}";
                    string convertTaskName = $"convert_{sample.id}";
                    string exportTaskName = $"export_{sample.id}";

                    taskDefinitions[importTaskName] = new ImportAzureBlobCreateRequest
                    {
                        Storage_Account = Config.StorageAccountName,
                        Container = Config.UploadStagingBlobContainerName,
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
                        Container = Config.UploadStagingBlobContainerName,
                        Sas_Token = exportSasToken,
                        Blob = sample.exportBlobName
                    };
                }

                var importImageSasToken = _storageService.GetSASTokenForUploadStagingBlob(samplePackRequest.importImgBlobName, BlobSasPermissions.Read);
                var exportImageSasToken = _storageService.GetSASTokenForUploadStagingBlob(samplePackRequest.exportImgBlobName, BlobSasPermissions.Read | BlobSasPermissions.Write);

                var importImageTaskName = "import_sample_pack_image";
                var convertImageTaskName = "convert_sample_pack_image";
                var exportImageTaskName = "export_sample-pack_image";

                //Sample Pack Thumbnail conversion
                taskDefinitions[importImageTaskName] = new ImportAzureBlobCreateRequest
                {
                    Storage_Account = Config.StorageAccountName,
                    Container = Config.UploadStagingBlobContainerName,
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
                    Container = Config.UploadStagingBlobContainerName,
                    Sas_Token = exportImageSasToken,
                    Blob = samplePackRequest.exportImgBlobName
                };

                var job = await cloudConvert.CreateJobAsync(new JobCreateRequest
                {
                    Tasks = taskDefinitions
                });
            }
            catch(Exception e)
            {
                var error = $"Failed to initiate cloud convert: {e.Message}";
                log.LogError(error, e);
                throw new Exception(error, e);
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
                Name = sampleMetadata.name,
                Description = $"{sampleMetadata.description}: {sampleDescriptions}",

                //TODO: Default to the currency of the User Account (Probably is affiliated w/ the account?)
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
            sampleMetadata.productId = stripeProduct.Id;
            sampleMetadata.priceId = stripeProduct.DefaultPriceId;
            sampleMetadata.sellerId = account.accountId;

            upsertSampleTransaction.request = sampleMetadata;

            return upsertSampleTransaction;
        }
    }
}
