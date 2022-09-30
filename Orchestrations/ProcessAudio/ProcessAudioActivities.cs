using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.util;

namespace vp.orchestrations.processaudio
{
    public static class ProcessAudioActivities
    {
        private static readonly IAudioProcessor videoProcessor = Utils.IsInDemoMode ?
            new MockAudioProcessor() : new FfmpegAudioProcessor();

        [FunctionName(ActivityNames.GetTranscodeProfiles)]
        public static TranscodeParams[] GetTranscodeProfiles(
            [ActivityTrigger] object input,
            ILogger log)
        {
            // TODO: Migrate this to the Config object...
            string transcodeProfiles = Environment.GetEnvironmentVariable("TranscodeProfiles");

            if (string.IsNullOrEmpty(transcodeProfiles))
            {
                return new[]
                {
                new TranscodeParams {
                    OutputExtension = ".ogg",
                    FfmpegParams = $"-b:a {Config.PreviewBitrate}k"
                }
            };
            }
            return JsonConvert.DeserializeObject<TranscodeParams[]>(transcodeProfiles);
        }

        [FunctionName(ActivityNames.TranscodeAudio)]
        public static async Task<string> TranscodAudio(
            [ActivityTrigger] TranscodeParams transcodeParams,
            ILogger log)
        {
            log.LogInformation($"Transcoding {transcodeParams.InputFile} with params " +
                     $"{transcodeParams.FfmpegParams} with extension {transcodeParams.OutputExtension}");

            return await videoProcessor.TranscodeAsync(transcodeParams, log);
        }

        [FunctionName(ActivityNames.StageAudioForTranscode)]
        public static async Task<string> StageAudioForTranscode(
            [ActivityTrigger] string[] paths,
            ILogger log)
        {
            // TODO: WTF IS THIS??? NOT APPROVED! FIX THIS... :D
            string localFilePath = paths[0];
            string fileName = paths[1];

            using (var fs = new FileStream(localFilePath, FileMode.Create))
            {
                BlockBlobClient blobClient = BlobFactory.MakeBlockBlobClient(Config.SampleBlobContainerName, fileName);
                await blobClient.DownloadToAsync(fs);
                return "Downloaded blob to local temp dir";
            }
        }

        [FunctionName(ActivityNames.PublishAudio)]
        public static async Task<string> PublishAudio(
            [ActivityTrigger] string incomingFile,
            ILogger log)
        {
            try
            {
                // TODO: Fix this drivel
                string blobName = incomingFile
                    .Substring(incomingFile.LastIndexOf('\\') + 1)
                    .Replace("\\", "")
                    .Replace("\"", "")
                    .Replace("]", "");

                BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(Config.SampleTranscodeContainerName);
                var blobClient = container.GetBlockBlobClient(blobName);

                log.LogInformation("Publishing mp3 preview");

                await videoProcessor.PublishAudio(incomingFile, blobClient);

                return "Preview Published Succesfully";
            }
            finally
            {
                Utils.TryDeleteFiles(log, new[] { incomingFile });
            }
        }
    }
}

