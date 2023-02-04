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
        public static async Task<SamplePack> UpsertSamplePack(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            SamplePack result;
            UpsertSamplePackTransaction upsertSamplePackTransaction = ctx.GetInput<UpsertSamplePackTransaction>();

            //TODO: Assign the instance Id to the image... (and also to each sample...) really???
            //ctx.InstanceId

            try
            {
                //TODO: Is this supposed to be context when all?
                var samples = await Task.WhenAll(
                    upsertSamplePackTransaction.request.sampleRequests.Select(
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

                var request = upsertSamplePackTransaction.request;
                var samplePack = new SamplePack
                {
                    _id = request._id,
                    name = request.name,
                    description = request.description,
                    samples = samples.Select(sample => sample).ToList()
                };
    
                result = await ctx.CallActivityWithRetryAsync<SamplePack>(
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
