using vp;
using vp.models;

namespace visiophone_cs_funcapp.Orchestrations.UpsertSamplePack
{
    public class UpsertSampleRequest : Sample
    {
        public string clipUri { get; set; }
        public string fileExtension { get; set; }
        public string samplePackId { get; set; } = "";

        public string importBlobName
        {
            get => $"{(samplePackId == "" ? "" : $"{samplePackId}/")}{Config.BlobImportDirectoryName}/{id}.{fileExtension}";
        }

        public string exportBlobName
        {
            get => $"{(samplePackId == "" ? "" : $"{samplePackId}/")}{Config.BlobExportDirectoryName}/{previewBlobName}";
        }

        public string previewBlobName
        {
            get => $"{id}.{Config.ClipExportFileFormat}";
        }

        public string sampleBlobName
        {
            get => $"{id}.{fileExtension}";
        }
    }
}
