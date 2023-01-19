using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.util;

namespace vp.orchestrations.processaudio
{
    public static class ProcessAudioOrchestrators
    {
        [FunctionName(OrchestratorNames.ProcessAudio)]
        public static async Task<object> ProcessAudio(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            ProcessAudioTransaction processAudioTransaction = ctx.GetInput<ProcessAudioTransaction>();
            try
            {
                string tempFolderPath = Utils.GetTempTranscodeFolder(ctx);

                processAudioTransaction.tempFolderPath = tempFolderPath;

                processAudioTransaction = await ctx.CallActivityWithRetryAsync<ProcessAudioTransaction>(
                    ActivityNames.StageAudioForTranscode,
                    new RetryOptions(TimeSpan.FromSeconds(5), 4),
                    processAudioTransaction);

                var transcodedLocations = await
                    ctx.CallSubOrchestratorAsync<string[]>(OrchestratorNames.Transcode, processAudioTransaction);

                foreach (string location in transcodedLocations)
                {
                    await ctx.CallActivityAsync(ActivityNames.PublishAudio, location);
                }

                return transcodedLocations;
            }
            catch (Exception e)
            {
                if (!ctx.IsReplaying)
                    log.LogError("Failed to process video with error " + e.Message);

                // TODO: Fix This....
                //await ctx.CallActivityAsync(ActivityNames.Cleanup, incomingFilename);
                return new { Error = "Failed to process video", e.Message };
            }
        }

        [FunctionName(OrchestratorNames.Transcode)]
        public static async Task<string[]> Transcode(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            ProcessAudioTransaction processAudioTransaction = ctx.GetInput<ProcessAudioTransaction>();

            var transcodeProfiles = await
                ctx.CallActivityAsync<TranscodeParams[]>(ActivityNames.GetTranscodeProfiles, null);

            var transcodeTasks = new List<Task<string>>();
            foreach (var transcodeProfile in transcodeProfiles)
            { 
                transcodeProfile.InputFile = processAudioTransaction.getTempFilePath();
                transcodeProfile.OutputFile = processAudioTransaction.getPreviewFilePath();
                var transcodeTask = ctx.CallActivityAsync<string>
                    (ActivityNames.TranscodeAudio, transcodeProfile);
                transcodeTasks.Add(transcodeTask);
            }
            var locations = await Task.WhenAll(transcodeTasks);
            return locations;
        }
    }
}

