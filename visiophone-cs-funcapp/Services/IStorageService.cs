using System;

namespace vp.services
{
    public interface IStorageService
    {
        Uri GetSASTokenForSampleBlob(string blobName);
    }
}
