using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.orchestrations.processaudio;

namespace vp.util {
    static class Utils
    {
        public static bool IsInDemoMode => Environment.GetEnvironmentVariable("DemoMode") == "true";

        //public static string GetTempTranscodeFolder(IDurableOrchestrationContext ctx, string path)
        //{
        //    var outputFolder = Path.Combine(Path.GetTempPath(), Config.SampleTranscodeContainerName, path);
        //    var inbound = Path.Combine(outputFolder, "inbound");
        //    var outbound = Path.Combine(outputFolder, "outbound");



        //    Directory.CreateDirectory(outputFolder);
        //    Directory.CreateDirectory(inbound);
        //    Directory.CreateDirectory(outbound);

        //    return outputFolder;
        //}

        public static string GetFileNameForId(string id, string incomingFileName) {
            return $"{id}.{GetExtensionForFileName(incomingFileName)}";
        }

        public static string GetExtensionForFileName(string filename) {
            string result = "";
            try
            {
                var parts = filename.Split('.');
                result = parts[parts.Length - 1];
            } catch
            {
                //consume
            }
            return result;

        }

        public static string GetReadSas(this BlobClient blob, TimeSpan validDuration)
        {
            var sas = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow + validDuration);
            return sas.ToString();
        }

        public static string GetFileExtension(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.'));
        }

        public static async Task<bool> CleanupTaskHub(
            IDurableOrchestrationClient client,
            ILogger log
        ) {
            await TerminateAllInstances(client, log);
            await Utils.PurgeWebJobHistory(client, DateTime.Now.Subtract(TimeSpan.FromDays(365)), DateTime.Now, log);

            return true;
        }

        public static async Task<bool> TerminateAllInstances(
            IDurableOrchestrationClient client,
            ILogger log
        ) {
            var instances = await GetAllInstances(client);
            foreach (var instance in instances)
            {
                try
                {
                    await TerminateInstance(client, instance);
                    log.LogInformation($"Deleted Instance: {JsonConvert.SerializeObject(instance)}");
                }
                catch (Exception e)
                {
                    log.LogInformation($"Delete Instance Failed: {e.Message}");

                }
            }
            return true;
        }

        public static async Task<List<DurableOrchestrationStatus>> GetAllInstances(
            IDurableOrchestrationClient client
        ) {
            var noFilter = new OrchestrationStatusQueryCondition();
            OrchestrationStatusQueryResult instances = await client.ListInstancesAsync(
                noFilter,
                CancellationToken.None);

            List<DurableOrchestrationStatus> result = new List<DurableOrchestrationStatus>();
            foreach (DurableOrchestrationStatus instance in instances.DurableOrchestrationState)
            {
                result.Add(instance);
            }
            return result;
        }

        public static async Task<bool> TerminateInstance(
            IDurableOrchestrationClient client,
            DurableOrchestrationStatus instance
        ) {
            string reason = "Found a bug";
            await client.TerminateAsync(instance.InstanceId, reason);
            return true;
        }


        public static async Task<bool> PurgeWebJobHistory(
            IDurableOrchestrationClient starter,
            DateTime from ,
            DateTime to,
            ILogger log)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var instances = await starter.ListInstancesAsync(
                new OrchestrationStatusQueryCondition
                {
                    CreatedTimeFrom = from,
                    CreatedTimeTo = to,
                },
                token
            );

            foreach (var eachInstance in instances.DurableOrchestrationState)
            {
                try
                {
                    await starter.PurgeInstanceHistoryAsync(eachInstance.InstanceId);
                }
                catch (Exception e)
                {
                    log.LogError($"Failed to purge instance {eachInstance.InstanceId}, {e.Message}", e);
                    return false;
                }
            }

            return true;
        }

        //private static HttpClient client;
        //public static async Task<string> DownloadToLocalFileAsync(string uri, IDurableOrchestrationContext ctx)
        //{
        //    var extension = Path.GetExtension(new Uri(uri).LocalPath);
        //    var outputFilePath = Path.Combine(GetTempTranscodeFolder(ctx), $"{Guid.NewGuid()}{extension}");
        //    client = client ?? new HttpClient();
        //    using (var downloadStream = await client.GetStreamAsync(uri))
        //    using (var s = File.OpenWrite(outputFilePath))
        //    {
        //        await downloadStream.CopyToAsync(s);
        //    }
        //    return outputFilePath;
        //}

        public static async Task<string> Transcode(TranscodeParams transcodeParams, ILogger log)
        {
            await FfmpegWrapper.Transcode(transcodeParams.InputFile, transcodeParams.FfmpegParams, transcodeParams.OutputFile, log);

            return transcodeParams.OutputFile;
        }

        public static bool UploadStream(Stream stream, string name, string containerName, string contentType)
        {
            BlockBlobClient blobClient = BlobFactory.MakeSampleBlockBlobClient(name, containerName);

            int offset = 0;
            int counter = 0;
            List<string> blockIds = new List<string>();

            var bytesRemaining = stream.Length;
            do
            {
                var dataToRead = Math.Min(bytesRemaining, Config.BufferSize);
                byte[] data = new byte[dataToRead];
                var dataRead = stream.Read(data, offset, (int)dataToRead);
                bytesRemaining -= dataRead;
                if (dataRead > 0)
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(counter.ToString("d6")));
                    blobClient.StageBlock(blockId, new MemoryStream(data));
                    Console.WriteLine(string.Format("Block {0} uploaded successfully.", counter.ToString("d6")));
                    blockIds.Add(blockId);
                    counter++;
                }
            } while (bytesRemaining > 0);

            // TODO should come from request
            var headers = new BlobHttpHeaders()
            {
                ContentType = contentType
            };
            blobClient.CommitBlockList(blockIds, headers);

            return true;
        }

        public static void UploadFormFile(IFormFile file, string containerName, string fileName)
        {
            using (var stream = file.OpenReadStream())
            {
                UploadStream(stream, $"{fileName}", containerName, file.ContentType);
            }
        }

        public static void TryDeleteFiles(ILogger log, params string[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    if (!string.IsNullOrEmpty(file) && File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception e)
                {
                    log.LogError($"Failed to clean up temporary file {file}", e);
                }
            }
        }
    }
}

