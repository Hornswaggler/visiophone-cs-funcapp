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
        public static async Task<SamplePack<Sample>> UpsertSamplePack(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            //SamplePack<Sample> result;
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();

            try
            {
                //TODO: Refactor to remove Side effects (transaction shouldn't be changing inside activity)
                //TODO: Change retries to someting configurable, longer than 5 seconds... :|
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

                //TODO: Tidy this up....
                upsertSamplePackTransaction.request.sellerId = upsertSamplePackTransaction.account.stripeId;
                upsertSamplePackTransaction.request.seller = upsertSamplePackTransaction.userName;

                ////TODO: Returning wrong data type...
                await ctx.CallActivityWithRetryAsync<SamplePack<Sample>>(
                    ActivityNames.UpsertSamplePackMetadata,
                    Config.OrchestratorRetryOptions,
                    (SamplePack<Sample>)upsertSamplePackTransaction.request
                );

                return (SamplePack<Sample>)upsertSamplePackTransaction.request;
            }
            catch (Exception e)
            {
                //Orchestration status should be "Failed... not "Complete""
                log.LogError($"Failed to process sampleRequest pack {upsertSamplePackTransaction.request.id}: {e.Message}, Rolling back transaction.", e);

                await ctx.CallSubOrchestratorWithRetryAsync<Sample>(
                    OrchestratorNames.RollbackSamplePackUpsert,
                    Config.OrchestratorRetryOptions,
                    upsertSamplePackTransaction
                );
            }
            return null;
        }
    }
}
