
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;

namespace vp.services
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _sampleSigningContainerClient;

        public StorageService() {
            Uri sampleCollectionContainerUri = new($"{Config.StorageBaseUrl}{Config.SampleCollectionName}");
            StorageSharedKeyCredential storageSharedKeyCredential = new(Config.StorageAccountName, Config.StorageAccountKey);

            _sampleSigningContainerClient  = new(sampleCollectionContainerUri, storageSharedKeyCredential);
        }

        public Uri GetSASTokenForSampleBlob(string blobName)
        {
            var blobClient = _sampleSigningContainerClient.GetBlobClient(blobName);
            
            Uri sasUri = null;
            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = "samples",
                    BlobName = blobName,
                    Resource = "b"
                };

                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri;
            }

            return null;
        }
    }
}
