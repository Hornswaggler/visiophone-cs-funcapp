using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableFunctionVideoProcessor;

interface IVideoProcessor
{
    Task<string> TranscodeAsync(TranscodeParams transcodeParams, BlobClient outputBlob, ILogger log, ExecutionContext context);
    Task<string> PrependIntroAsync(BlobClient outputBlob, string introLocation, string incomingFile, ILogger log);
    Task<string> ExtractThumbnailAsync(string incomingFile, BlobClient outputBlob, ILogger log);
    Task PublishVideo(string videoLocations, BlockBlobClient blobClient);
    Task RejectVideo(string[] videoLocations);
}