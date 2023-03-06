using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.util;

namespace vp.orchestrations.processaudio
{
    //TODO: ENSURE THE OPERATION(S) that occur on local disk are in a single orchestration / sub orch
    //Otherwise idempotency is broken!
    public static class ProcessAudioActivities
    {
        private static readonly IAudioProcessor videoProcessor = Utils.IsInDemoMode ?
            new MockAudioProcessor() : new FfmpegAudioProcessor();

        [FunctionName(ActivityNames.GetTranscodeProfiles)]
        public static ProcessAudioTransaction GetTranscodeProfiles(
            [ActivityTrigger] ProcessAudioTransaction processAudioTransaction,
            ILogger log)
        {
            // TODO: Migrate this to the Config object...
            //string transcodeProfiles = Environment.GetEnvironmentVariable("TranscodeProfiles");

            processAudioTransaction.transcodeProfiles.Add(
                new TranscodeParams
                {
                    OutputExtension = Config.SamplePreviewFileFormat,
                    FfmpegParams = $"-b:a {Config.PreviewBitrate}k"
                }
            );
          
            return processAudioTransaction;
        }

        [FunctionName(ActivityNames.TranscodeAudio)]
        public static async Task<string> TranscodAudio(
            [ActivityTrigger] TranscodeParams transcodeParams,
            ILogger log)
        {
            log.LogInformation($"Transcoding {transcodeParams.InputFile} with params " +
                $"{transcodeParams.FfmpegParams} with extension {transcodeParams.OutputExtension}");

            string result = "";
            try
            {
                result = await videoProcessor.TranscodeAsync(transcodeParams, log);
            } catch (Exception e)
            {
                log.LogCritical($"Failed to transcode media: {e.Message}", e);
            }
            

            return result;
        }

        [FunctionName(ActivityNames.StageAudioForTranscode)]
        public static async Task<ProcessAudioTransaction> StageAudioForTranscode(
            [ActivityTrigger] ProcessAudioTransaction processAudioTransaction,
            ILogger log)
        {
            var tempFilePath = processAudioTransaction.getTempFilePath();
            using (var fs = new FileStream(processAudioTransaction.getTempFilePath(), FileMode.Create))
            {
                BlockBlobClient blobClient = BlobFactory.MakeBlockBlobClient(
                    Config.SampleBlobContainerName, 
                    processAudioTransaction.incomingFileName);

                await blobClient.DownloadToAsync(fs);
                processAudioTransaction.tempFilePath = tempFilePath;
                return processAudioTransaction;
            }
        }

        [FunctionName(ActivityNames.PublishAudio)]
        public static async Task<ProcessAudioTransaction> PublishAudio(
            [ActivityTrigger] ProcessAudioTransaction processAudioTransaction,
            ILogger log)
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            try
            {
                BlobContainerClient previewContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleTranscodeContainerName);
                BlobContainerClient sampleContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleFilesContainerName);

                await Task.WhenAll(processAudioTransaction.transcodeProfiles.Select(
                    profile =>
                    {
                        var blobClient = previewContainer.GetBlockBlobClient($"{processAudioTransaction.sampleId}{profile.OutputExtension}");
                        return videoProcessor.PublishAudio(profile.OutputFile, blobClient);
                    }
                ).Concat(processAudioTransaction.transcodeProfiles.Select(
                    profile =>
                    {
                        var blobClient = sampleContainer.GetBlockBlobClient($"{processAudioTransaction.sampleId}.{processAudioTransaction.fileExtension}");
                        return videoProcessor.PublishAudio(profile.InputFile, blobClient);
                    }
                )));
            }
            catch (Exception e)
            {
                //TODO: Rollback :|
                var error = $"FAILED to publish audio: {e.Message}";
                log.LogError($"FAILED to publish audio: {e.Message}", e);
                throw new Exception(error, e);
            }
            finally {
                try
                {
                    foreach(var profile in processAudioTransaction.transcodeProfiles)
                    {
                        Utils.TryDeleteFiles(log, new[] { profile.InputFile });
                        Utils.TryDeleteFiles(log, new[] { profile.OutputFile });
                    }
  
                }catch (Exception e)
                {
                    //TODO: Rollback :|
                    var error = $"FAILED to cleanup files!: {e.Message}";
                    log.LogError(error, e);
                    throw new Exception(error, e);
                }

            }

            return processAudioTransaction;
        }
    }
}

