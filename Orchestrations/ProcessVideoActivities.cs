using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using DurableFunctionVideoProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace visiophone_cs_funcapp.Orchestrations;

public static class ProcessVideoActivities
{
    private static readonly IVideoProcessor videoProcessor = Utils.IsInDemoMode ?
        new MockVideoProcessor() : new FfmpegVideoProcessor();

    [FunctionName(ActivityNames.GetTranscodeProfiles)]
    public static TranscodeParams[] GetTranscodeProfiles(
        [ActivityTrigger] object input,
        ILogger log)
    {
        //var bitrates = Environment.GetEnvironmentVariable("TranscodeProfiles");
        //if (String.IsNullOrEmpty(bitrates))
        return new[]
        {
                new TranscodeParams {
                    OutputExtension = ".mp3",
                    FfmpegParams = "-b:a 256k"
                }
            };
        //return JsonConvert.DeserializeObject<TranscodeParams[]>(bitrates);
    }

    [FunctionName(ActivityNames.TranscodeVideo)]
    public static async Task<string> TranscodeVideo(
        [ActivityTrigger] TranscodeParams transcodeParams,

        ILogger log, ExecutionContext context)
    {
        var outputBlobName = Path.GetFileNameWithoutExtension(transcodeParams.InputFile) +
                             transcodeParams.OutputExtension;
        log.LogInformation($"Transcoding {transcodeParams.InputFile} with params " +
                 $"{transcodeParams.FfmpegParams} with extension {transcodeParams.OutputExtension}");

        //var outputBlob = dir.GetBlobClient($"transcoded/{outputBlobName}");

        return await videoProcessor.TranscodeAsync(transcodeParams, null, log, context);
    }

    [FunctionName(ActivityNames.ExtractThumbnail)]
    public static async Task<string> ExtractThumbnail(
        [ActivityTrigger] string[] paths,

        ILogger log)
    {
        string localFilePath = paths[0];
        string fileName = paths[1];

        using (var fs = new FileStream(localFilePath, FileMode.Create))
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            BlobServiceClient _blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient("uploads");
            BlockBlobClient blobClient = container.GetBlockBlobClient(fileName);
            await blobClient.DownloadToAsync(fs);
            return "YES!";
        }
    }

    [FunctionName(ActivityNames.Cleanup)]
    public static async Task<string> Cleanup(
        [ActivityTrigger] string incomingFile,
        ILogger log)
    {
        log.LogInformation($"Cleaning up {incomingFile}");
        await Task.Delay(5000); // simulate some work
        return "Finished";
    }

    [FunctionName(ActivityNames.PublishVideo)]
    public static async Task<string> PublishVideo(
        [ActivityTrigger] string incomingFile,
        ILogger log)
    {
        try
        {
            string blobName = incomingFile
                .Substring(incomingFile.LastIndexOf('\\') + 1).Replace("\\", "").Replace("\"", "").Replace("]", "");

            BlobServiceClient _blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient("transcoded");
            var blobClient = container.GetBlockBlobClient(blobName);

            log.LogInformation("Publishing video");

            await videoProcessor.PublishVideo(incomingFile, blobClient);

            return "The video is live";
        }
        finally {
            Utils.TryDeleteFiles(log, new [] { incomingFile });
        }

    }

    [FunctionName(ActivityNames.RejectVideo)]
    public static async Task<string> RejectVideo(
        [ActivityTrigger] string[] mediaLocations,
        ILogger log)
    {
        log.LogInformation("Rejecting video");
        await videoProcessor.RejectVideo(mediaLocations);
        return "All temporary files have been deleted";
    }


    [FunctionName(ActivityNames.PeriodicActivity)]
    public static void PeriodicActivity(
        [ActivityTrigger] int timesRun,
        ILogger log)
    {
        log.LogInformation($"Running the periodic activity {timesRun}");
    }


}
