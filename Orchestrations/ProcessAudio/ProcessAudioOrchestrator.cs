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
            string incomingFilename = ctx.GetInput<string>();
            try
            {
                string tempFolderPath = Utils.GetTempTranscodeFolder(ctx);
                string localFilePath = $"{tempFolderPath}\\{incomingFilename}";

                await ctx.CallActivityWithRetryAsync<string>(ActivityNames.StageAudioForTranscode,
                    new RetryOptions(TimeSpan.FromSeconds(5), 4),
                    new[] { localFilePath, incomingFilename });

                var transcodedLocations = await
                    ctx.CallSubOrchestratorAsync<string[]>(OrchestratorNames.Transcode, new[] { incomingFilename, tempFolderPath });

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
            var paths = ctx.GetInput<string[]>();
            string localFilePath = paths[0];
            string tempFolderPath = paths[1];

            var transcodeProfiles = await
                ctx.CallActivityAsync<TranscodeParams[]>(ActivityNames.GetTranscodeProfiles, null);


            var transcodeTasks = new List<Task<string>>();
            foreach (var transcodeProfile in transcodeProfiles)
            { 
                transcodeProfile.InputFile = tempFolderPath + "\\" + localFilePath;
                transcodeProfile.OutputFile = tempFolderPath + "\\" + Guid.NewGuid() + ".mp3";
                var transcodeTask = ctx.CallActivityAsync<string>
                    (ActivityNames.TranscodeAudio, transcodeProfile);
                transcodeTasks.Add(transcodeTask);
            }
            var locations = await Task.WhenAll(transcodeTasks);
            return locations;
        }
    }
}

