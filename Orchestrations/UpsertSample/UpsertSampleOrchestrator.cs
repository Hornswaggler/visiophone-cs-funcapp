using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.models;
using vp.orchestrations.processaudio;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSample)]
        public static async Task<Sample> UpsertSample (
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            UpsertSampleTransaction transaction = ctx.GetInput<UpsertSampleTransaction>();
            ProcessAudioTransaction audioTransaction = new ProcessAudioTransaction
            {
                incomingFileName = transaction.request.sampleFileName
            };

            var processAudioResult = await ctx.CallSubOrchestratorAsync<ProcessAudioTransaction>(
                OrchestratorNames.ProcessAudio,
                audioTransaction
            );

            transaction = await ctx.CallActivityWithRetryAsync<UpsertSampleTransaction>(
                ActivityNames.UpsertStripeData,
                new RetryOptions(TimeSpan.FromSeconds(5), 4),
                transaction);

            var request = transaction.request;
            var sample = SampleFactory.MakeSampleForSampleRequest(transaction.request);
            var result = await ctx.CallActivityWithRetryAsync<Sample>(
                ActivityNames.UpsertSample,
                new RetryOptions(TimeSpan.FromSeconds(5), 1),
                sample
            );

            return result;
        }
    }
}
