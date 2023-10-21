
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private ILogger<StorageService> _log;

        private readonly BlobContainerClient _sampleHDBlobContainerClient;
        private readonly BlobContainerClient _samplePreviewBlobContainerClient;
        private readonly BlobContainerClient _uploadStagingBlobContainerClient;
        private readonly BlobContainerClient _samplePackCoverArtBlobContainerClient;
        private readonly BlobContainerClient _userAvatarBlobContainerClient;
        
        public StorageService(ILogger<StorageService> log) {
            _log = log; 

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

        private void UploadFileToBlob(IFormFile file, BlockBlobClient blobClient)
        {
            using (var stream = file.OpenReadStream())
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

                        _log.LogTrace(string.Format("Block {0} uploaded successfully.", counter.ToString("d6")));

                        blockIds.Add(blockId);
                        counter++;
                    }
                } while (bytesRemaining > 0);

                var headers = new BlobHttpHeaders()
                {
                    ContentType = file.ContentType
                };
                blobClient.CommitBlockList(blockIds, headers);
            }
        }

        public async Task<Response<bool>[]> CleanupUploadDataForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            return await Task.WhenAll(DeleteUploadsForSamplePackTransaction(upsertSamplePackTransaction));
        }

        public async Task<Response<bool>[]> RollbackSampleTransactionBlobsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction) {
            return await Task.WhenAll(
                DeleteUploadsForSamplePackTransaction(upsertSamplePackTransaction)
                    .Concat(DeleteHDSamplesForSamplePackTransaction(upsertSamplePackTransaction)
                    .Concat(DeletePreviewSamplesForSamplePackTransaction(upsertSamplePackTransaction))
                    .Append(DeleteCoverArtForSamplePackTransaction(upsertSamplePackTransaction))
                )
            );
        }

        public async Task<CopyFromUriOperation[]> MigrateUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            var samplePackCoverArtBlob = _samplePackCoverArtBlobContainerClient.GetBlobClient(upsertSamplePackTransaction.request.imgBlobName);

            List<Task<CopyFromUriOperation>> copyTasks = new List<Task<CopyFromUriOperation>>
            {
                samplePackCoverArtBlob.StartCopyFromUriAsync(GetSASURIForUploadStagingBlob(upsertSamplePackTransaction.request.exportImgBlobName))
            };

            return await Task.WhenAll(
                copyTasks.Concat(
                    upsertSamplePackTransaction.request.samples.Select(
                        sampleRequest =>
                        {
                            var samplePreviewBlob = _samplePreviewBlobContainerClient.GetBlobClient(sampleRequest.previewBlobName);
                            return samplePreviewBlob.StartCopyFromUriAsync(GetSASURIForUploadStagingBlob(sampleRequest.exportBlobName));

                        }
                    )
                )
                .Concat(
                    upsertSamplePackTransaction.request.samples.Select(
                        sampleRequest =>
                        {

                            var sampleHDBlob = _sampleHDBlobContainerClient.GetBlobClient(sampleRequest.sampleBlobName);
                            return sampleHDBlob.StartCopyFromUriAsync(GetSASURIForUploadStagingBlob(sampleRequest.importBlobName));
                        }
                    )
                )
            );
        }

        private IEnumerable<Task<Response<bool>>> DeleteUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            List<string> blobs = new List<string>
            {
                upsertSamplePackTransaction.request.importImgBlobName,
                upsertSamplePackTransaction.request.exportImgBlobName
            };

            foreach (var sampleTransaction in upsertSamplePackTransaction.request.samples)
            {
                blobs.Add(sampleTransaction.importBlobName);
                blobs.Add(sampleTransaction.exportBlobName);
            }

            return blobs.Select(blob => _uploadStagingBlobContainerClient.DeleteBlobIfExistsAsync(blob, DeleteSnapshotsOption.IncludeSnapshots));
        }


        private IEnumerable<Task<Response<bool>>> DeleteHDSamplesForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            return upsertSamplePackTransaction.request.samples.Select(
                blob => _sampleHDBlobContainerClient.DeleteBlobIfExistsAsync(blob.sampleBlobName, DeleteSnapshotsOption.IncludeSnapshots)
            );
        }

        private IEnumerable<Task<Response<bool>>> DeletePreviewSamplesForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            return upsertSamplePackTransaction.request.samples.Select(
                blob => _samplePreviewBlobContainerClient.DeleteBlobIfExistsAsync(blob.previewBlobName, DeleteSnapshotsOption.IncludeSnapshots)
            );
        }

        private Task<Response<bool>> DeleteCoverArtForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction)
        {
            return _samplePackCoverArtBlobContainerClient.DeleteBlobIfExistsAsync(upsertSamplePackTransaction.request.imgBlobName, DeleteSnapshotsOption.IncludeSnapshots);
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
