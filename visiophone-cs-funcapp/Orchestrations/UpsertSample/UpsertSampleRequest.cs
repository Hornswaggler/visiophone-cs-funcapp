using vp.models;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleRequest : Sample
    {
        public string clipUri { get; set; }
        public string blobName { get; set; } = "";
        public string fileExtension { get; set; }

        public string importBlobName {
            get => $"{Config.BlobImportDirectoryName}/{id}.{fileExtension}";
        }

        public string exportClipBlobName
        {
            get => $"{Config.BlobExportDirectoryName}/{id}.{Config.ClipExportFileFormat}";
        }
    }
}
