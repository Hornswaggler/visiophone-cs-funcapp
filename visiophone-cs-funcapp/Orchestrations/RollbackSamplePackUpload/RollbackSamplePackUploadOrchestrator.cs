using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.orchestrations.upsertSamplePack;
using vp.util;

namespace vp.orchestrations.rollbackSamplePackUploadOrchestrator
{
    public class RollbackSamplePackUploadOrchestrator
    {
        [FunctionName(OrchestratorNames.RollbackSamplePackUpsert)]
        public static async Task RollbackSamplePackUpsert(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();

            var blobClient = BlobFactory.GetBlobContainerClient(Config.SampleBlobContainerName);

            List<string> blobs = new List<string>();
            blobs.Add(upsertSamplePackTransaction.request.importImgBlobName);
            blobs.Add(upsertSamplePackTransaction.request.exportImgBlobName);

            foreach (var sampleTransaction in upsertSamplePackTransaction.request.samples)
            {
                blobs.Add(sampleTransaction.importBlobName);
            }
            
            //TODO: replace with Task When All...
            foreach(var blob in blobs)
            {
                await blobClient.DeleteBlobIfExistsAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);
            }

            return;
        }
    }
}
