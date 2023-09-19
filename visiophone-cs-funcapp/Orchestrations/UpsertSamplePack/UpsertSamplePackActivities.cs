﻿using Azure;
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
            throw new Exception("The shit hit the fan! Everything is sideways");



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

        [FunctionName(ActivityNames.ConvertSamplePackAssets)]
        public async Task<UpsertSamplePackTransaction> ConvertSamplePackAssets(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction, ILogger log)
        {
            var cloudConvert = new CloudConvertAPI(Config.CloudConvertAPIKey);

            try
            {
                dynamic taskDefinitions = new Dictionary<string, object>();

                //Sample Conversion(s)
                foreach (var sample in upsertSamplePackTransaction.request.samples)
                {
                    string importSampleFileName = $"{upsertSamplePackTransaction.request.id}/{sample.importBlobName}";
                    string exportSampleFileName = $"{upsertSamplePackTransaction.request.id}/{sample.exportClipBlobName}";

                    //TODO: ... UGH
                    var importSasToken = $"?{_storageService.GetSASTokenForUploadBlob(importSampleFileName, BlobSasPermissions.Read).ToString().Split('?').Last()}";
                    var exportSasToken = $"?{_storageService.GetSASTokenForUploadBlob(exportSampleFileName, BlobSasPermissions.Read | BlobSasPermissions.Write).ToString().Split('?').Last()}";

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
                        Sas_Token = exportSasToken.ToString(),
                        Blob = exportSampleFileName
                    };
                }

                //Sample Pack Thumbnail conversion
                //taskDefinitions["import_sample_pack_image"] = new ImportAzureBlobCreateRequest
                //{
                //    Storage_Account = Config.StorageAccountName,
                //    Container = Config.SampleBlobContainerName,
                //    Sas_Token = importSas
                //}

                //TODO: do not await... let callbacks handle this
                var job = await cloudConvert.CreateJobAsync(new JobCreateRequest
                {
                    Tasks = taskDefinitions
                });
            }
            catch(Exception e)
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
