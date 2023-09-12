
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;

//Test

namespace vp.services
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _sampleSigningContainerClient;
        private readonly BlobContainerClient _transcodeContainerClient;
        private readonly BlobContainerClient _uploadContainerClient;

        public StorageService() {
            StorageSharedKeyCredential storageSharedKeyCredential = new(Config.StorageAccountName, Config.StorageAccountKey);
            
            //TODO: These must be mapped via front door...
            Uri sampleCollectionContainerUri = new($"https://visiophonewebstoreage.blob.core.windows.net/{Config.SampleCollectionName}");
            _sampleSigningContainerClient = new(sampleCollectionContainerUri, storageSharedKeyCredential);

            Uri transcodeContainerUri = new($"https://visiophonewebstoreage.blob.core.windows.net/{Config.SampleTranscodeContainerName}");
            _transcodeContainerClient = new(transcodeContainerUri, storageSharedKeyCredential);

            Uri uploadContainerUri = new($"https://visiophonewebstoreage.blob.core.windows.net/{Config.UploadStagingContainerName}");
            _uploadContainerClient = new(uploadContainerUri, storageSharedKeyCredential);
        }

        public Uri GetSASTokenForTranscodeBlob(string blobName, BlobSasPermissions permissions) {
            return GetSASTokenForBlob(_transcodeContainerClient, blobName, permissions);
        }

        public Uri GetSASTokenForSampleBlob(string blobName, BlobSasPermissions permissions)
        {
            return GetSASTokenForBlob(_sampleSigningContainerClient, blobName, permissions);
        }

        public Uri GetSASTokenForUploadBlob(string blobName, BlobSasPermissions permissions)
        {
            return GetSASTokenForBlob(_uploadContainerClient, blobName, permissions);
        }

        private Uri GetSASTokenForBlob(BlobContainerClient container, string blobName, BlobSasPermissions permissions)
        {
            var blobClient = container.GetBlobClient(blobName);
            
            Uri sasUri = null;
            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = container.Name,
                    BlobName = blobName,
                    Resource = "b"
                };

                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(permissions);

                sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri;
            }

            return null;
        }
    }
}
