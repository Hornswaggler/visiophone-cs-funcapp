using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace vp.util
{
    public class BlockBlobClientFactory
    {
        public static BlockBlobClient MakeBlockBlobClient(string containerName, string blobName)
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);
            return container.GetBlockBlobClient(blobName);
        }

        public static BlockBlobClient MakeSampleBlockBlobClient(string blobName)
        {
            return MakeBlockBlobClient(Config.SampleBlobContainerName, blobName);
        }
    }
}