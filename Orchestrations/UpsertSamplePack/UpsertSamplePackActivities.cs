using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using vp.models;
using vp.services;
using vp.util;

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


        [FunctionName(ActivityNames.UpsertSamplePackTransferImage)]
        public async Task<UpsertSamplePackTransaction> UpsertSamplePackTransferImage(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
        )
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient destContainer = _blobServiceClient.GetBlobContainerClient(Config.CoverArtContainerName);
            var destBlob = destContainer.GetBlobClient(upsertSamplePackTransaction.request.imageFileName);
            
            await destBlob.StartCopyFromUriAsync(BlobFactory.GetBlobSasToken(Config.UploadStagingContainerName, upsertSamplePackTransaction.request.imageFileName));

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.CleanupStagingData)]
        public async Task<UpsertSamplePackTransaction> CleanupStagingData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
)
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient srcContainer = _blobServiceClient.GetBlobContainerClient(Config.UploadStagingContainerName);
            var imgBlob = srcContainer.GetBlobClient(upsertSamplePackTransaction.request.imageFileName);

            await imgBlob.DeleteIfExistsAsync();

            foreach(var sampleRequest in upsertSamplePackTransaction.request.sampleRequests)
            {
                var sampleBlob = srcContainer.GetBlobClient(sampleRequest.sampleFileName);
                await sampleBlob.DeleteIfExistsAsync();
            }

            return upsertSamplePackTransaction;
        }
    }
}
