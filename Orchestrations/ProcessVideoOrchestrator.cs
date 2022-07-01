using System;
using System.Collections.Generic;
//using System.IO;
//using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using Azure.Storage.Blobs;
//using Azure.Storage.Blobs.Specialized;
using DurableFunctionVideoProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace visiophone_cs_funcapp.Orchestrations;

public static class ProcessVideoOrchestrators
{
    [FunctionName(OrchestratorNames.ProcessAudio)]
    public static async Task<object> ProcessAudio(
        [OrchestrationTrigger] IDurableOrchestrationContext ctx,
        ILogger log)
    {
        string incomingFilename = ctx.GetInput<string>();
        try
        {
            string localFilePath = $"{Utils.GetTempTranscodeFolder()}\\{incomingFilename}";

            string thumbnailLocation = await ctx.CallActivityWithRetryAsync<string>(ActivityNames.ExtractThumbnail,
                new RetryOptions(TimeSpan.FromSeconds(5), 4),
                new[] { localFilePath, incomingFilename });

            var transcodedLocations = await
                ctx.CallSubOrchestratorAsync<string[]>(OrchestratorNames.Transcode, localFilePath);

            var transcodedLocation = transcodedLocations[0];

            foreach (string location in transcodedLocations)
            {
                await ctx.CallActivityAsync(ActivityNames.PublishVideo, location);
            }

            return "UGH";
        }
        catch (Exception e)
        {
            if (!ctx.IsReplaying)
                log.LogError("Failed to process video with error " + e.Message);
            await ctx.CallActivityAsync(ActivityNames.Cleanup, incomingFilename);
            return new { Error = "Failed to process video", e.Message };
        }
    }

    [FunctionName(OrchestratorNames.Transcode)]
    public static async Task<string[]> Transcode(
        [OrchestrationTrigger] IDurableOrchestrationContext ctx,
        ILogger log)
    {
        var localFilePath = ctx.GetInput<string>();

        var transcodeProfiles = await
            ctx.CallActivityAsync<TranscodeParams[]>(ActivityNames.GetTranscodeProfiles, null);
        var transcodeTasks = new List<Task<string>>();
        foreach (var transcodeProfile in transcodeProfiles)
        {

            transcodeProfile.InputFile = localFilePath;
            var transcodeTask = ctx.CallActivityAsync<string>
                (ActivityNames.TranscodeVideo, transcodeProfile);
            transcodeTasks.Add(transcodeTask);
        }
        var locations = await Task.WhenAll(transcodeTasks);
        return locations;
    }

    [FunctionName(OrchestratorNames.PeriodicTask)]
    public static async Task<int> PeriodicTask(
        [OrchestrationTrigger] IDurableOrchestrationContext ctx,
        ILogger log)
    {
        var timesRun = ctx.GetInput<int>();
        timesRun++;
        if (!ctx.IsReplaying)
            log.LogInformation($"Starting the PeriodicTask orchestrator {ctx.InstanceId}, {timesRun}");
        await ctx.CallActivityAsync(ActivityNames.PeriodicActivity, timesRun);
        var nextRun = ctx.CurrentUtcDateTime.AddSeconds(30);
        await ctx.CreateTimer(nextRun, CancellationToken.None);
        ctx.ContinueAsNew(timesRun);
        return timesRun;
    }
}
