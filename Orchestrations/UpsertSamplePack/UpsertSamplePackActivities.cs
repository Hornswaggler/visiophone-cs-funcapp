using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using vp.models;
using vp.services;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackActivities
    {
        private static ISamplePackService _samplePackService;

        public UpsertSamplePackActivities(ISamplePackService samplePackService)
        {
            _samplePackService = samplePackService;
        }

        [FunctionName(ActivityNames.UpsertSamplePackMetadata)]
        public async Task<SamplePack> UpsertSamplePackMetadata(
            [ActivityTrigger] SamplePack samplePack)
        {
            var result = await _samplePackService.AddSamplePack(samplePack);
            return result;
        }
    }
}
