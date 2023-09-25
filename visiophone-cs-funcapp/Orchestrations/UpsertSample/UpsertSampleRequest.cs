using vp.models;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleRequest : Sample
    {
        public string clipUri { get; set; }
        public string fileExtension { get; set; }
        public string samplePackId { get; set; } = "";

        public string importBlobName {
            get => $"{(samplePackId == "" ? "" : $"{samplePackId}/")}{Config.BlobImportDirectoryName}/{id}.{fileExtension}";
        }

        //TODO: Fix this :| 
        public string exportBlobName
        {
            get => $"{(samplePackId == "" ? "" : $"{samplePackId}/")}{Config.BlobExportDirectoryName}/{previewBlobName}";
        }

        //public string exportBlobName {
        //    get => $"{importBlobName}";
        //}

        public string previewBlobName
        {
            get => $"{id}.{Config.ClipExportFileFormat}";
        }

        //public string previewBlobName
        //{
        //    get => sampleBlobName;
        //}

        public string sampleBlobName
        {
            get => $"{id}.{fileExtension}";
        }
    }
}
