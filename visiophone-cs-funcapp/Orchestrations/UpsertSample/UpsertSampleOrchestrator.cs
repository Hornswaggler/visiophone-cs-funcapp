using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.models;

namespace vp.orchestrations.upsertsample
{
    //TODO: This isn't currently being used...
    public class UpsertSampleOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSample)]
        public static async Task<Sample> UpsertSample(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            //OLD CODE... :|
            log.LogInformation($"Processing instance: {ctx.InstanceId}");

            UpsertSampleTransaction transaction = ctx.GetInput<UpsertSampleTransaction>();

            var sample = SampleFactory.MakeSampleForSampleRequest(transaction.request);

            return sample;
        }
    }
}
