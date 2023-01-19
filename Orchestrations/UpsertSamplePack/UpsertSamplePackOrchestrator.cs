using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSamplePack)]
        public static async Task<object> UpsertSamplePack(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();
            try
            {
                await Task.WhenAll(
                    upsertSamplePackTransaction.sampleRequests.Select(
                        sampleRequest =>
                        {
                            sampleRequest.sampleMetadata.sellerId = upsertSamplePackTransaction.account.Id;
                            sampleRequest.sampleMetadata.seller = upsertSamplePackTransaction.userName;

                            return ctx.CallSubOrchestratorAsync<UpsertSampleTransaction>(
                                OrchestratorNames.UpsertSample,
                                new UpsertSampleTransaction(
                                    upsertSamplePackTransaction.account,
                                    sampleRequest));
                        }
                    )
                );
            } catch(Exception e)
            {
                log.LogError("failed to process sample pack", e);
                //TODO: rollback transaction here
            }

            return upsertSamplePackTransaction;
        }
    }
}
