using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.orchestrations.processaudio;

namespace vp.util {
    static class Utils
    {
        public static bool IsInDemoMode => Environment.GetEnvironmentVariable("DemoMode") == "true";

        public static string GetTempTranscodeFolder(IDurableOrchestrationContext ctx)
        {
            var outputFolder = Path.Combine(Path.GetTempPath(), Config.SampleTranscodeContainerName, $"{ctx.CurrentUtcDateTime:yyyy-MM-dd}");
            Directory.CreateDirectory(outputFolder);
            return outputFolder;
        }

        public static string GetReadSas(this BlobClient blob, TimeSpan validDuration)
        {
            var sas = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow + validDuration);
            return sas.ToString();
        }

        private static HttpClient client;
        public static async Task<string> DownloadToLocalFileAsync(string uri, IDurableOrchestrationContext ctx)
        {
            var extension = Path.GetExtension(new Uri(uri).LocalPath);
            var outputFilePath = Path.Combine(GetTempTranscodeFolder(ctx), $"{Guid.NewGuid()}{extension}");
            client = client ?? new HttpClient();
            using (var downloadStream = await client.GetStreamAsync(uri))
            using (var s = File.OpenWrite(outputFilePath))
            {
                await downloadStream.CopyToAsync(s);
            }
            return outputFilePath;
        }

        public static async Task<string> Transcode(TranscodeParams transcodeParams, ILogger log)
        {
            await FfmpegWrapper.Transcode(transcodeParams.InputFile, transcodeParams.FfmpegParams, transcodeParams.OutputFile, log);

            return transcodeParams.OutputFile;
        }

        public static bool UploadStream(Stream stream, string name, string containerName, string contentType)
        {
            BlockBlobClient blobClient = BlockBlobClientFactory.MakeSampleBlockBlobClient(name, containerName);
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

        public static void UploadFormFileAsync(IFormFile file, string containerName, string fileName)
        {
            string fileExtension = file.FileName.Split('.')[1];
            using (var stream = file.OpenReadStream())
            {
                UploadStream(stream, $"{fileName}.{fileExtension}", containerName, file.ContentType);
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

