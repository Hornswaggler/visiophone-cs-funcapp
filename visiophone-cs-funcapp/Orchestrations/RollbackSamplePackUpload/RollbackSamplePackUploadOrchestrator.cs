using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using vp.orchestrations.upsertSamplePack;

namespace vp.orchestrations.rollbackSamplePackUploadOrchestrator
{
    public class RollbackSamplePackUploadOrchestrator
    {
        [FunctionName(OrchestratorNames.RollbackSamplePackUpsert)]
        public static async Task<UpsertSamplePackTransaction> RollbackSamplePackUpsert(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();
            var result = upsertSamplePackTransaction;
            try
            {
                result = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.RollbackSamplePackUpload,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                result = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.RollbackStripeProduct,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                result = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.RollbackSamplePackMetadata,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );
            }
            catch(Exception e)
            {
                log.LogError($"Failed to rollback sample pack upload: {upsertSamplePackTransaction.request.id}.", e);
            }

            return result;
        }
    }
}
