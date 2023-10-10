
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vp.orchestrations.upsertSamplePack;

namespace vp.services
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _sampleHDBlobContainerClient;
        private readonly BlobContainerClient _samplePreviewBlobContainerClient;
        private readonly BlobContainerClient _uploadStagingBlobContainerClient;
        private readonly BlobContainerClient _samplePackCoverArtBlobContainerClient;
        private readonly BlobContainerClient _userAvatarBlobContainerClient;
        
        public StorageService() {
            StorageSharedKeyCredential storageSharedKeyCredential = new(Config.StorageAccountName, Config.StorageAccountKey);

            Uri sampleHDBlobContainerUri = new($"{Config.StorageBaseUrl}/{Config.SampleHDBlobContainerName}");
            Uri samplePreviewBlobContainerUri = new($"{Config.StorageBaseUrl}/{Config.SamplePreviewBlobContainerName}");
            Uri uploadStagingBlobContainerUri = new($"{Config.StorageBaseUrl}/{Config.UploadStagingBlobContainerName}");
            Uri samplePackCoverArtBlobContainerUri = new($"{Config.StorageBaseUrl}/{Config.SamplePackCoverArtBlobContainerName}");
            Uri userAvatarBlobContainerUri = new($"{Config.StorageBaseUrl}/{Config.AvatarBlobContainerName}");

            _uploadStagingBlobContainerClient = new(uploadStagingBlobContainerUri, storageSharedKeyCredential);
            _samplePreviewBlobContainerClient = new(samplePreviewBlobContainerUri, storageSharedKeyCredential);
            _sampleHDBlobContainerClient = new(sampleHDBlobContainerUri, storageSharedKeyCredential);
            _samplePackCoverArtBlobContainerClient = new(samplePackCoverArtBlobContainerUri, storageSharedKeyCredential);
            _userAvatarBlobContainerClient = new(userAvatarBlobContainerUri, storageSharedKeyCredential);
        }

        public Uri GetSASURIForSamplePreviewBlob(string blobName, BlobSasPermissions permissions = BlobSasPermissions.Read) {
            return GetSASTokenForBlob(_samplePreviewBlobContainerClient, blobName, permissions);
        }

        public Uri GetSASURIForSampleHDBlob(string blobName, BlobSasPermissions permissions = BlobSasPermissions.Read)
        {
            return GetSASTokenForBlob(_sampleHDBlobContainerClient, blobName, permissions);
        }

        public Uri GetSASURIForUploadStagingBlob(string blobName, BlobSasPermissions permissions = BlobSasPermissions.Read)
        {
            return GetSASTokenForBlob(_uploadStagingBlobContainerClient, blobName, permissions);
        }

        public string GetSASTokenForUploadStagingBlob(string blobName, BlobSasPermissions permissions = BlobSasPermissions.Read)
        {
            return $"?{GetSASURIForUploadStagingBlob(blobName, permissions).ToString().Split('?').Last()}";
        }

        public void UploadUserAvatar(IFormFile file, string blobName) {
            UploadFileToBlob(file, _userAvatarBlobContainerClient.GetBlockBlobClient(blobName));
        }

        public void UploadStagingBlob(IFormFile file, string blobName) {
            UploadFileToBlob(file, _uploadStagingBlobContainerClient.GetBlockBlobClient(blobName));
        }

        private void UploadFileToBlob(IFormFile file, BlockBlobClient blobClient) {
            using (var stream = file.OpenReadStream())
            {
                UploadStream(stream, file.ContentType, blobClient);
            }
        }

        private void UploadStream(Stream stream, string contentType, BlockBlobClient blobClient)
        {
            int offset = 0;
            int counter = 0;
            List<string> blockIds = new List<string>();

            var bytesRemaining = stream.Length;
            do
            {
                var dataToRead = Math.Min(bytesRemaining, Config.BufferSize);
                byte[] data = new byte[dataToRead];
                var dataRead = stream.Read(data, offset, (int)dataToRead);
                bytesRemaining -= dataRead;
                if (dataRead > 0)
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(counter.ToString("d6")));
                    blobClient.StageBlock(blockId, new MemoryStream(data));
                    //TODO: Shouldn't be console logging...
                    Console.WriteLine(string.Format("Block {0} uploaded successfully.", counter.ToString("d6")));
                    blockIds.Add(blockId);
                    counter++;
                }
            } while (bytesRemaining > 0);

            // TODO should come from request
            var headers = new BlobHttpHeaders()
            {
                ContentType = contentType
            };
            blobClient.CommitBlockList(blockIds, headers);
        }

        public async Task<bool> DeleteUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction) {
            List<string> blobs = new List<string>();
            blobs.Add(upsertSamplePackTransaction.request.importImgBlobName);
            blobs.Add(upsertSamplePackTransaction.request.exportImgBlobName);

            foreach (var sampleTransaction in upsertSamplePackTransaction.request.samples)
            {
                blobs.Add(sampleTransaction.importBlobName);
                blobs.Add(sampleTransaction.exportBlobName);
            }

            await Task.WhenAll(
                blobs.Select(
                    blob =>
                    {
                        return _uploadStagingBlobContainerClient.DeleteBlobIfExistsAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);
                    }
                )
            );

            return true;
        }

        public async Task<bool> MigrateUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            var samplePackRequest = upsertSamplePackTransaction.request;
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            //Migrate Samples to preview and high quality containers
            foreach (var sampleRequest in samplePackRequest.samples)
            {
                var samplePreviewBlob = _samplePreviewBlobContainerClient.GetBlobClient(sampleRequest.previewBlobName);
                await samplePreviewBlob.StartCopyFromUriAsync(GetSASURIForUploadStagingBlob(sampleRequest.exportBlobName));

                var sampleHDBlob = _sampleHDBlobContainerClient.GetBlobClient(sampleRequest.sampleBlobName);
                await sampleHDBlob.StartCopyFromUriAsync(GetSASURIForUploadStagingBlob(sampleRequest.importBlobName));
            }

            //Migrate image to preview container
            var samplePackCoverArtBlob = _samplePackCoverArtBlobContainerClient.GetBlobClient(samplePackRequest.imgBlobName);
            await samplePackCoverArtBlob.StartCopyFromUriAsync(GetSASURIForUploadStagingBlob(samplePackRequest.exportImgBlobName));

            return true;
        }

        //public async Task<bool> DeleteUploadTranscodeForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        //{

        //    //_samplePreviewBlobContainerClient.
        //}

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
