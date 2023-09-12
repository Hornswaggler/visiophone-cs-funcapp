﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;
using vp.models;
using vp.orchestrations.upsertsample;
using Azure.ResourceManager.Media;
using Azure.ResourceManager;
using Azure.Core;
using Azure.Identity;
using RetryOptions = Microsoft.Azure.WebJobs.Extensions.DurableTask.RetryOptions;
using Azure;
using Azure.ResourceManager.Media.Models;
using Azure.Storage.Blobs;
using System.Net;
using System.IO;
using CloudConvert.API;
using CloudConvert.API.Models.ExportOperations;
using CloudConvert.API.Models.ImportOperations;
using CloudConvert.API.Models.JobModels;
using CloudConvert.API.Models.TaskOperations;
using System.Collections.Generic;
using vp.services;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackOrchestrator
    {
        IStorageService _storageService;

        [FunctionName(OrchestratorNames.UpsertSamplePack)]
        public static async Task<SamplePack<Sample>> UpsertSamplePack(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            SamplePack<Sample> result;
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();



            ////TODO: Move credentials stuff to "Util"
            //var mediaServicesResourceId = MediaServicesAccountResource.CreateResourceIdentifier(
            //    subscriptionId: Config.StorageSubscriptionId,
            //    resourceGroupName: Config.StorageResourceGroupName,
            //    accountName: Config.StorageAccountName
            //);

            //var credential = new DefaultAzureCredential();
            //var armClient = new ArmClient(credential);
            //var mediaServicesAccount = armClient.GetMediaServicesAccountResource(mediaServicesResourceId);

            //MediaAssetResource asset;


            //foreach (var sample in upsertSamplePackTransaction.request.samples)
            //{
            //    string assetName = $"{sample.id}.wav";
            //    try
            //    {
            //        //TODO: Hardcoded file type...
            //        asset = await mediaServicesAccount.GetMediaAssets().GetAsync(assetName);

            //        // The Asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
            //        // an existing Asset, use an unique name.
            //        Console.WriteLine($"Warning: The Asset named {assetName} already exists. It will be overwritten.");
            //    }
            //    catch (RequestFailedException)
            //    {
            //        // Call Media Services API to create an Asset.
            //        // This method creates a container in storage for the Asset.
            //        // The files (blobs) associated with the Asset will be stored in this container.
            //        Console.WriteLine("Creating an input Asset...");
            //        asset = (await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(WaitUntil.Completed, assetName, new MediaAssetData())).Value;
            //    }

            //    //CLOUD CONVERT TEST CODE...
            //    var CloudConvert = new CloudConvertAPI(Config.CloudConvertAPIKey);


            //    var sasUriCollection = asset.GetStorageContainerUrisAsync(
            //        new MediaAssetStorageContainerSasContent
            //        {
            //            Permissions = MediaAssetContainerPermission.Read,
            //            ExpireOn = DateTime.UtcNow.AddHours(1)
            //        }).GetAsyncEnumerator();
            //    await sasUriCollection.MoveNextAsync();
            //    var sasUri = sasUriCollection.Current;

            //    _storageService.GetSASTokenForSampleBlob($"{sample.id}.wav")


            //    var job = await CloudConvert.CreateJobAsync(new JobCreateRequest
            //    {
            //        Tasks = new
            //        {
            //            import_it = new ImportAzureBlobCreateRequest
            //            {
            //                Storage_Account = Config.StorageAccountName,
            //                Container = Config.SampleBlobContainerName,
            //                Sas_Token = sasUri.ToString()
            //            },
            //            convert = new ConvertCreateRequest
            //            {
            //                Input = "import_it",
            //                Input_Format = "wav",
            //                Output_Format = "mp3",
            //                Engine = "ffmpeg",
            //                Options = new Dictionary<string, object> {
            //                    ["audio_codec"] = "mp3",
            //                    ["audio_qscale"] = 0
            //                }

            //            },
            //            export_it = new ExportUrlCreateRequest
            //            {
            //                Input = "convert"
            //            }
            //        }
            //    });
            //}

            //END CLOUD CONVERT TEST CODE...

            try
            {
                //TODO: in progress... call batch process here...
                //foreach(var sampleRequest in upsertSamplePackTransaction.request.samples)
                //{
                //    await MediaServicesTranscode(sampleRequest);
                //}


                //TODO: OLD STUFF again... :|
                var samples = await Task.WhenAll(
                    upsertSamplePackTransaction.request.samples.Select(
                        sampleRequest =>
                        {
                            log.LogInformation($"samplePack: {upsertSamplePackTransaction.request.id}, sampleRequest: {sampleRequest.id}");

                            return ctx.CallSubOrchestratorAsync<Sample>(
                                OrchestratorNames.UpsertSample,
                                (new UpsertSampleTransaction(
                                    upsertSamplePackTransaction.account,
                                    sampleRequest,
                                    upsertSamplePackTransaction.request.id)));
                        }
                    )
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.InitiateCloudConvertJob,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );





                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertSamplePackTransferImage,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                //upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                //    ActivityNames.CleanupStagingData,
                //    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                //    upsertSamplePackTransaction
                //);

                // COMBINE FOR IDEMPOTENCY
                /////////////////////////////


                //TODO: Trigger this from the cloud convert web HOOK!

                //Generate Price Id in Stripe for Sample Pack
                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertStripeData,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                var request = upsertSamplePackTransaction.request;
                var samplePack = new SamplePack<Sample>
                {
                    id = request.id,
                    name = request.name,
                    cost = request.cost,
                    priceId = request.priceId,
                    description = request.description,
                    samples = samples.Select(sample => sample).ToList(),
                    sellerId = upsertSamplePackTransaction.account.stripeId,
                    seller = upsertSamplePackTransaction.userName
                };

                result = await ctx.CallActivityWithRetryAsync<SamplePack<Sample>>(
                    ActivityNames.UpsertSamplePackMetadata,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    samplePack
                );


                ///////////////////////////////

                return result;
            }
            catch (Exception e)
            {
                log.LogError($"Failed to process sampleRequest pack: {e.Message}", e);
                //TODO: rollback transaction here
            }
            return null;
        }

        public static async Task MediaServicesTranscode(UpsertSampleRequest sampleRequest) {

            var mediaServicesResourceId = MediaServicesAccountResource.CreateResourceIdentifier(
                subscriptionId: "e1896010-0921-499e-a947-f5aef5306277",
                resourceGroupName: "visophone-east-us2",
                accountName: "visiophonemediaservices"
            );

            var credential = new DefaultAzureCredential();
            var armClient = new ArmClient(credential);

            var mediaServicesAccount = armClient.GetMediaServicesAccountResource(mediaServicesResourceId);

            //// In this example, we are assuming that the Asset name is unique.
            MediaAssetResource asset;

            string assetName = "sounds.wav";
            try
            {
                asset = await mediaServicesAccount.GetMediaAssets().GetAsync(assetName);

                // The Asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
                // an existing Asset, use an unique name.
                Console.WriteLine($"Warning: The Asset named {assetName} already exists. It will be overwritten.");
            }
            catch (RequestFailedException)
            {
                // Call Media Services API to create an Asset.
                // This method creates a container in storage for the Asset.
                // The files (blobs) associated with the Asset will be stored in this container.
                Console.WriteLine("Creating an input Asset...");
                asset = (await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(WaitUntil.Completed, assetName, new MediaAssetData())).Value;
            }

            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            // That is where you would specify read-write permissions 
            // and the expiration time for the SAS URL.
            var sasUriCollection = asset.GetStorageContainerUrisAsync(
                new MediaAssetStorageContainerSasContent
                {
                    Permissions = MediaAssetContainerPermission.ReadWrite,
                    ExpireOn = DateTime.UtcNow.AddHours(1)
                }).GetAsyncEnumerator();

            await sasUriCollection.MoveNextAsync();
            var sasUri = sasUriCollection.Current;

            //// Use Storage API to get a reference to the Asset container
            //// that was created by calling Asset's CreateOrUpdate method.
            var container = new BlobContainerClient(sasUri);
            BlobClient blob = container.GetBlobClient("sounds.wav");

            using WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            using Stream stream = client.OpenRead("https://www.visiophone.wtf/transcoded/flyfartsonice.wav");

            //// Use Storage API to upload the file into the container in storage.
            //Console.WriteLine("Uploading a media file to the Asset...");
            await blob.UploadAsync(stream);

            stream.Close();

            //return asset;
        }
    }
}
