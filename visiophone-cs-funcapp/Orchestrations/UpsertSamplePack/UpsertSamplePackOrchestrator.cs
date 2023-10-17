using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.models;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSamplePack)]
        public static async Task<UpsertSamplePackTransaction> UpsertSamplePack(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();

            try
            {
                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.ConvertSamplePackAssets,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.MigrateSamplePackAssets,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.CleanupStagingData,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertStripeData,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertSamplePackMetadata,
                    Config.OrchestratorRetryOptions,
                    (SamplePack<Sample>)upsertSamplePackTransaction.request
                );
            }
            catch (Exception e)
            {
                var error = $"Failed to process sampleRequest pack {upsertSamplePackTransaction.request.id}: {e.Message}, Rolling back transaction.";
                log.LogError(error, e);

                await ctx.CallSubOrchestratorWithRetryAsync<Sample>(
                    OrchestratorNames.RollbackSamplePackUpsert,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );

                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }
    }
}
