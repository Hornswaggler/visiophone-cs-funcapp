using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;
using vp.models;
using vp.orchestrations.upsertsample;

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
                var samples = await Task.WhenAll(
                    upsertSamplePackTransaction.request.samples.Select(
                        sampleRequest =>
                        {
                            log.LogInformation($"samplePack: {upsertSamplePackTransaction.request.id}, sample: {sampleRequest.id}");

                            return ctx.CallSubOrchestratorAsync<Sample>(
                                OrchestratorNames.UpsertSample,
                                (new UpsertSampleTransaction(
                                    upsertSamplePackTransaction.account,
                                    sampleRequest,
                                    upsertSamplePackTransaction.request.id)));
                        }
                    )
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertSamplePackTransferImage,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                //upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                //    ActivityNames.CleanupStagingData,
                //    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                //    upsertSamplePackTransaction
                //);

                // COMBINE FOR IDEMPOTENCY
                /////////////////////////////

                //Generate Price Id in Stripe for Sample Pack
                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertStripeData,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                var request = upsertSamplePackTransaction.request;
                var samplePack = new SamplePack<Sample>
                {
                    id = request.id,
                    name = request.name,
                    cost = request.cost,
                    priceId = request.priceId,
                    description = request.description,
                    samples = samples.Select(sample => sample).ToList(),
                    sellerId = upsertSamplePackTransaction.account.stripeId,
                    seller = upsertSamplePackTransaction.userName
                };

                result = await ctx.CallActivityWithRetryAsync<SamplePack<Sample>>(
                    ActivityNames.UpsertSamplePackMetadata,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    samplePack
                );


                ///////////////////////////////

                return result;
            }
            catch (Exception e)
            {
                log.LogError($"Failed to process sample pack: {e.Message}", e);
                //TODO: rollback transaction here
            }
            return null;
        }
    }
}
