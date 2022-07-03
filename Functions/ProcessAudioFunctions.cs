using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace vp {
    public static class ProcessAudioFunctions
    {
        [FunctionName(nameof(AutoProcessUploadedVideos))]
        public static async Task AutoProcessUploadedVideos(
            [BlobTrigger("uploads/{name}", Connection = "STORAGE_CONNECTION_STRING")] Stream myBlob, string name,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            try
            {
                string id = new Guid().ToString();
                var orchestrationId = await starter.StartNewAsync<string>(
                    OrchestratorNames.ProcessAudio, name);

                log.LogInformation($"Started an orchestration {orchestrationId} for uploaded audio: {name}");
            }
            catch (Exception e)
            {
                log.LogError(new Exception("Failed:", e), "Start Orchestration Failed");
            }
        }
    }
}

