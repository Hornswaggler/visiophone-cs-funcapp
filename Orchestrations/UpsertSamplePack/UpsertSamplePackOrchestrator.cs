using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;
using vp.models;
using vp.orchestrations.upsertsample;
using Newtonsoft.Json;
using System.Collections.Generic;

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
                //TODO: Is this supposed to be context when all?
                var samples = await Task.WhenAll(
                    upsertSamplePackTransaction.request.samples.Select(
                        sampleRequest =>
                        {
                            sampleRequest.sellerId = upsertSamplePackTransaction.account.Id;
                            sampleRequest.seller = upsertSamplePackTransaction.userName;

                            return ctx.CallSubOrchestratorAsync<Sample>(
                                OrchestratorNames.UpsertSample,
                                (new UpsertSampleTransaction(
                                    upsertSamplePackTransaction.account,
                                    sampleRequest)));
                        }
                    )
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.UpsertSamplePackTransferImage,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                upsertSamplePackTransaction = await ctx.CallActivityWithRetryAsync<UpsertSamplePackTransaction>(
                    ActivityNames.CleanupStagingData,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    upsertSamplePackTransaction
                );

                var request = upsertSamplePackTransaction.request;
                var samplePack = new SamplePack<Sample>
                {
                    _id = request._id,
                    name = request.name,
                    description = request.description,
                    samples = samples.Select(sample => sample).ToList()
                };
    
                result = await ctx.CallActivityWithRetryAsync<SamplePack<Sample>>(
                    ActivityNames.UpsertSamplePackMetadata,
                    new RetryOptions(TimeSpan.FromSeconds(5), 1),
                    samplePack
                );

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
