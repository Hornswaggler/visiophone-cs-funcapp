using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vp.orchestrations.upsertSamplePack;
using vp.util;

namespace vp.orchestrations.rollbackSamplePackUploadOrchestrator
{
    public class RollbackSamplePackUploadActivities
    {
        [FunctionName(ActivityNames.RollbackSamplePackUpload)]
        public async Task<UpsertSamplePackTransaction> RollbackSamplePackUpload(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                var blobClient = BlobFactory.GetBlobContainerClient(Config.SampleBlobContainerName);

                List<string> blobs = new List<string>();
                blobs.Add(upsertSamplePackTransaction.request.importImgBlobName);
                blobs.Add(upsertSamplePackTransaction.request.exportImgBlobName);

                foreach (var sampleTransaction in upsertSamplePackTransaction.request.samples)
                {
                    blobs.Add(sampleTransaction.importBlobName);
                    blobs.Add(sampleTransaction.exportBlobName);
                }

                await Task.WhenAll(
                    blobs.Select(
                        blob =>
                        {
                            return blobClient.DeleteBlobIfExistsAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);
                        }
                    )
                );

                return upsertSamplePackTransaction;
            } catch(Exception e)
            {
                var error = $"Failed to rollback sample pack upload for request: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }
            
        }
    }
}
