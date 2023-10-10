using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
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
        Task<bool> DeleteUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction);
        Task<bool> MigrateUploadsForSamplePackTransaction(UpsertSamplePackTransaction upsertSamplePackTransaction);
    }
}
