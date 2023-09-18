using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;

namespace vp.util
{
    public class BlobFactory
    {
        public static BlobContainerClient GetBlobContainerClient(string containerName) {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }

        public static BlockBlobClient MakeBlockBlobClient(string containerName, string blobName)
        {
            //BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
            //BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);

            //BlobContainerSasPermissions permissions = new BlobContainerSasPermissions
            //{
            //}

            ////BlobSasBuilder sasBuilder = new BlobSasBuilder()
            ////{
            ////    BlobContainerName = containerName,
            ////    Resource = "b"
            ////};

            ////sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(2);
            ////sasBuilder.SetPermissions(BlobSasPermissions.Read);

            ////////Uri sasUri = container.GenerateSasUri(sasBuilder);
            ///////

            ////container.GetBlobClient(blobName).GenerateSasUri(sasBuilder);

            return GetBlobContainerClient(containerName).GetBlockBlobClient(blobName);
        }

        public static Uri GetBlobSasToken(string containerName, string blobName)
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                Resource = "b"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(2);
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return container.GetBlobClient(blobName).GenerateSasUri(sasBuilder);
        }

        public static BlockBlobClient MakeSampleBlockBlobClient(string blobName, string containerName)
        {
            return MakeBlockBlobClient(containerName, blobName);
        }
    }
}