using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace vp
{
    class MockAudioProcessor : IAudioProcessor
    {
        public Task PublishVideo(string[] videoLocations)
        {
            return Task.Delay(5000);
        }

        public Task PublishAudio(string videoLocations, BlockBlobClient blobClient)
        {
            throw new NotImplementedException();
        }

        public Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log, IDurableOrchestrationContext ctx)
        {
            return (Task<string>)Task.Delay(5000);
        }

        public Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
