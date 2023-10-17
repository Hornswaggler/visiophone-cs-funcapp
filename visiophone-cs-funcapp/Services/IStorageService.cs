using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using vp.orchestrations.upsertSamplePack;

namespace vp.services
{
    public interface IStorageService
    {
        Uri GetSASURIForSampleHDBlob(string blobName, BlobSasPermissions permissions);
        Uri GetSASURIForSamplePreviewBlob(string blobName, BlobSasPermissions permissions);
        Uri GetSASURIForUploadStagingBlob(string blobName, BlobSasPermissions permissions);
        string GetSASTokenForUploadStagingBlob(string blobName, BlobSasPermissions permissions);
        void UploadUserAvatar(IFormFile file, string blobName);
        void UploadStagingBlob(IFormFile file, string blobName);
        Task<CopyFromUriOperation[]> MigrateUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction);
        Task<Response<bool>[]> CleanupUploadDataForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction);
        Task<Response<bool>[]> RollbackSampleTransactionBlobsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction);
    }
}
