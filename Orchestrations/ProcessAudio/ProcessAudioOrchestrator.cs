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
        public static async Task<ProcessAudioTransaction> ProcessAudio(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            ProcessAudioTransaction processAudioTransaction;
            try
            {
                processAudioTransaction = ctx.GetInput<ProcessAudioTransaction>();
                string tempFolderPath = Utils.GetTempTranscodeFolder(ctx);

                processAudioTransaction.tempFolderPath = tempFolderPath;

                processAudioTransaction = await ctx.CallActivityWithRetryAsync<ProcessAudioTransaction>(
                    ActivityNames.StageAudioForTranscode,
                    new RetryOptions(TimeSpan.FromSeconds(5), 4),
                    processAudioTransaction
                );


                processAudioTransaction = await ctx.CallSubOrchestratorAsync<ProcessAudioTransaction>(
                    OrchestratorNames.Transcode, 
                    processAudioTransaction
                );

                foreach (string location in processAudioTransaction.transcodePaths)
                {
                    await ctx.CallActivityAsync(ActivityNames.PublishAudio, location);
                }

                return processAudioTransaction;
            }
            catch (Exception e)
            {
                if (!ctx.IsReplaying)
                    log.LogError("Failed to process video with error " + e.Message);

                processAudioTransaction = new ProcessAudioTransaction();

                // TODO: Fix This....
                //await ctx.CallActivityAsync(ActivityNames.Cleanup, incomingFilename);
                processAudioTransaction.errors.Add($"Failed to process video: {e.Message}");

                return processAudioTransaction;
            }
        }

        [FunctionName(OrchestratorNames.Transcode)]
        public static async Task<ProcessAudioTransaction> Transcode(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            ProcessAudioTransaction processAudioTransaction;
            try
            {
                processAudioTransaction = ctx.GetInput<ProcessAudioTransaction>();
                processAudioTransaction = await ctx.CallActivityAsync<ProcessAudioTransaction>(ActivityNames.GetTranscodeProfiles, processAudioTransaction);

                var transcodeTasks = new List<Task<string>>();
                foreach (var transcodeProfile in processAudioTransaction.transcodeProfiles)
                {
                    transcodeProfile.InputFile = processAudioTransaction.getTempFilePath();
                    transcodeProfile.OutputFile = processAudioTransaction.getPreviewFilePath();
                    var transcodeTask = ctx.CallActivityAsync<string>
                        (ActivityNames.TranscodeAudio, transcodeProfile);
                    transcodeTasks.Add(transcodeTask);
                }
                var locations = await Task.WhenAll(transcodeTasks);

                foreach(string location in locations)
                {
                    processAudioTransaction.transcodePaths.Add(location);
                }

                return processAudioTransaction;
            } catch (Exception e)
            {
                if (!ctx.IsReplaying)
                    log.LogError("Failed to process video with error " + e.Message);

                processAudioTransaction = new ProcessAudioTransaction();

                // TODO: Fix This....
                //await ctx.CallActivityAsync(ActivityNames.Cleanup, incomingFilename);
                processAudioTransaction.errors.Add($"Failed to transcode video: {e.Message}");

                return processAudioTransaction;
            }
            
        }
    }
}

