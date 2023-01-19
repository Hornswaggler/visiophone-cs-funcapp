using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSample)]
        public static async Task<UpsertSampleTransaction> UpsertSample (
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            UpsertSampleTransaction metadata = ctx.GetInput<UpsertSampleTransaction>();
            ProcessAudioTransaction audioTransaction = new ProcessAudioTransaction
            {
                incomingFileName = metadata.request.sampleFileName
            };

            var processAudioResult = await ctx.CallSubOrchestratorAsync<string[]>(
                OrchestratorNames.ProcessAudio,
                audioTransaction
            );

            metadata = await ctx.CallActivityWithRetryAsync<UpsertSampleTransaction>(
                ActivityNames.UpsertStripeData,
                new RetryOptions(TimeSpan.FromSeconds(5), 4),
                metadata);

            metadata = await ctx.CallActivityWithRetryAsync<UpsertSampleTransaction>(ActivityNames.UpsertSampleMetaData,
                new RetryOptions(TimeSpan.FromSeconds(5), 4),
                metadata);

            return metadata;
        }



    }
}
