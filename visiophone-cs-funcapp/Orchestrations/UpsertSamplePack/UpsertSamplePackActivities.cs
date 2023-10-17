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
        public static async Task<UpsertSamplePackTransaction> UpsertSamplePackMetadata(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Inserting Sample Pack Metadata");

                upsertSamplePackTransaction.request.sellerId = upsertSamplePackTransaction.account.stripeId;
                upsertSamplePackTransaction.request.seller = upsertSamplePackTransaction.userName;

                await _samplePackService.AddSamplePack((SamplePack<Sample>)upsertSamplePackTransaction.request);
            }
            catch (Exception e) {
                var error = $"Sample Pack Metadata insert failed for transaction: {upsertSamplePackTransaction.request.id}";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.MigrateSamplePackAssets)]
        public async Task<UpsertSamplePackTransaction> MigrateSamplePackAssets(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log
        )
        {
            try
            {
                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Migrating Samplepack Assets");
                await _storageService.MigrateUploadsForSamplePackTransaction(upsertSamplePackTransaction);
            } catch ( Exception e )
            {
                var error = $"Sample Pack Asset Migration failed for transaction: {upsertSamplePackTransaction.request.id}";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.CleanupStagingData)]
        public async Task<UpsertSamplePackTransaction> CleanupStagingData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Cleaning up staging data");
                await _storageService.CleanupUploadDataForSamplePackTransaction(upsertSamplePackTransaction);
            } catch ( Exception e )
            {
                var error = $"Sample Pack Asset staging data cleanup failed for transaction: {upsertSamplePackTransaction.request.id}";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.ConvertSamplePackAssets)]
        public async Task<UpsertSamplePackTransaction> ConvertSamplePackAssets(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction, ILogger log)
        {
            try
            {
                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Converting Samplepack Assets");

                var cloudConvert = new CloudConvertAPI(Config.CloudConvertAPIKey);
                var samplePackRequest = upsertSamplePackTransaction.request;

                dynamic taskDefinitions = new Dictionary<string, object>();

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

                //TODO: This should be a webhook callback, it is causing a blocking operation
                var status = await cloudConvert.WaitJobAsync(job.Data.Id);

            }
            catch (Exception e)
            {
                var error = $"Sample Conversion Process Failed for transaction {upsertSamplePackTransaction.request.id}";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.UpsertStripeData)]
        public static async Task<UpsertSamplePackTransaction> UpsertStripeData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Inserting Stripe data");

                var sampleDescriptions = upsertSamplePackTransaction.request.samples.Aggregate("", (acc, sample) =>
                {
                    return $"{(acc == "" ? "" : $"{acc}, ")}{sample.name}";
                });

                var options = new Stripe.ProductCreateOptions
                {
                    Name = upsertSamplePackTransaction.request.name,
                    Description = $"{upsertSamplePackTransaction.request.description}: {sampleDescriptions}",

                    //TODO: Default to the currency of the User Account (Probably is affiliated w/ the account?)
                    DefaultPriceData = new Stripe.ProductDefaultPriceDataOptions
                    {
                        Currency = upsertSamplePackTransaction.account.defaultCurrency,
                        UnitAmountDecimal = upsertSamplePackTransaction.request.cost
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "accountId", $"{upsertSamplePackTransaction.account.stripeId}" },
                        { "id", $"{upsertSamplePackTransaction.request.id}" },
                        { "type", "samplePacks"}
                    }
                };

                var service = new Stripe.ProductService();
                var stripeProduct = await service.CreateAsync(options);

                upsertSamplePackTransaction.request.productId = stripeProduct.Id;
                upsertSamplePackTransaction.request.priceId = stripeProduct.DefaultPriceId;
                upsertSamplePackTransaction.request.sellerId = upsertSamplePackTransaction.account.accountId;
            }
            catch (Exception e)
            {
                var error = $"Sample Pack Stripe data upload failed for transaction {upsertSamplePackTransaction.request.id}";
                log.LogError(error, e);
                throw new Exception(error, e);
            }
            
            return upsertSamplePackTransaction;
        }
    }
}
