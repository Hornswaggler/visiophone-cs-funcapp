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
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );

                if(upsertSamplePackTransaction.request.priceId == null || upsertSamplePackTransaction.request.productId == null)
                {
                    result = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                        ActivityNames.RollbackStripeProduct,
                        Config.OrchestratorRetryOptions,
                        upsertSamplePackTransaction
                    );
                }
                
                result = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.RollbackSamplePackMetadata,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );
            }
            catch(Exception e)
            {
                var error = $"Failed to rollback sample pack upload: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return result;
        }
    }
}
