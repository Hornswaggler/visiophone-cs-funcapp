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
                fileExtension = transaction.request.fileExtension,
                sampleId = transaction.request._id,
                incomingFileName = transaction.request.clipUri
            };

            var processAudioResult = await ctx.CallSubOrchestratorAsync<ProcessAudioTransaction>(
                OrchestratorNames.ProcessAudio,
                audioTransaction
            );

            var sample = SampleFactory.MakeSampleForSampleRequest(transaction.request);

            return sample;
        }
    }
}
