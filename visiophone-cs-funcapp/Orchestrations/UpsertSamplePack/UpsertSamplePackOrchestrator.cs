using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;
using vp.models;
using vp.orchestrations.upsertsample;
using RetryOptions = Microsoft.Azure.WebJobs.Extensions.DurableTask.RetryOptions;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSamplePack)]
        public static async Task<SamplePack<Sample>> UpsertSamplePack(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            SamplePack<Sample> result;
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();

            try
            {
                //TODO: The file upload(s) are atomic
                //TODO: Delete this... not being used anymore :|
                var samples = await Task.WhenAll(
                    upsertSamplePackTransaction.request.samples.Select(
                        sampleRequest =>
                        {
                            log.LogInformation($"samplePack: {upsertSamplePackTransaction.request.id}, sampleRequest: {sampleRequest.id}");

                            return ctx.CallSubOrchestratorAsync<Sample>(
                                OrchestratorNames.UpsertSample,
                                (new UpsertSampleTransaction(
                                    upsertSamplePackTransaction.account,
                                    sampleRequest,
                                    upsertSamplePackTransaction.request.id)));
                        }
                    )
                );

                //TODO: Change retries to someting configurable, longer than 5 seconds... :|
                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Converting Samplepack Assets");
                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.ConvertSamplePackAssets,
                    new RetryOptions(TimeSpan.FromSeconds(5), 3),
                    upsertSamplePackTransaction
                );

                log.LogInformation($"Processing transaction: {upsertSamplePackTransaction.request.id}, Migrating Samplepack Assets");
                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.MigrateSamplePackAssets,
                    new RetryOptions(TimeSpan.FromSeconds(5), 3),
                    upsertSamplePackTransaction
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.CleanupStagingData,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                //Generate Price Id in Stripe for Sample Pack
                //TODO: Fix this, if it re-runs it may create multiple products in stripe...
                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertStripeData,
                    new RetryOptions(TimeSpan.FromSeconds(10), 2),
                    upsertSamplePackTransaction
                );

                //TODO: this "conversion" should occur inside the activity, not here in the orchestration
                var request = upsertSamplePackTransaction.request;
                var samplePack = new SamplePack<Sample>
                {
                    id = request.id,
                    name = request.name,
                    cost = request.cost,
                    priceId = request.priceId,
                    productId = request.productId,
                    description = request.description,
                    samples = samples.Select(sample => sample).ToList(),
                    sellerId = upsertSamplePackTransaction.account.stripeId,
                    seller = upsertSamplePackTransaction.userName
                };
                
                //TODO: Returning wrong data type...
                result = await ctx.CallActivityWithRetryAsync<SamplePack<Sample>>(
                    ActivityNames.UpsertSamplePackMetadata,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    samplePack
                );

                //throw new Exception("Blow everything up...");

                return result;
            }
            catch (Exception e)
            {
                //Orchestration status should be "Failed... not "Complete""
                log.LogError($"Failed to process sampleRequest pack {upsertSamplePackTransaction.request.id}: {e.Message}, Rolling back transaction.", e);

                await ctx.CallSubOrchestratorWithRetryAsync<Sample>(
                    OrchestratorNames.RollbackSamplePackUpsert,
                    new RetryOptions(TimeSpan.FromSeconds(5), 20),
                    upsertSamplePackTransaction
                );
            }
            return null;
        }
    }
}
