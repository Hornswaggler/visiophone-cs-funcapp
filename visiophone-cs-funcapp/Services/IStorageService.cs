using Azure.Storage.Sas;
using System;

namespace vp.services
{
    public interface IStorageService
    {
        Uri GetSASTokenForSampleBlob(string blobName, BlobSasPermissions permissions);
        Uri GetSASTokenForTranscodeBlob(string blobName, BlobSasPermissions permissions);
        Uri GetSASTokenForUploadBlob(string blobName, BlobSasPermissions permissions);
    }
}
