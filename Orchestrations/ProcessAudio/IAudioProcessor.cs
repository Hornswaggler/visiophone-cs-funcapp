using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace vp.orchestrations.processaudio{
    interface IAudioProcessor
    {
        Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log);

        Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log, IDurableOrchestrationContext ctx);
        Task PublishAudio(string videoLocations, BlockBlobClient blobClient);
    }
}

