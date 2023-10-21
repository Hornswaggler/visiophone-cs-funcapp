using visiophone_cs_funcapp.Orchestrations.UpsertSamplePack;
using vp.models;
using vp.util;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackRequest : SamplePack<UpsertSampleRequest>
    {
        public string imgUrl { get; set; }

        public string imgUrlExtension
        {
            get => Utils.GetExtensionForFileName(imgUrl);
        }

        public string imgBlobName
        {
            get => $"{id}.{exportImgBlobExtension}";
        }

        public string importImgBlobName
        {
            get => $"{id}/{Config.BlobImportDirectoryName}/{id}.{imgUrlExtension}";
        }

        public string exportImgBlobName
        {
            get => $"{id}/{Config.BlobExportDirectoryName}/{imgBlobName}";
        }

        public string exportImgBlobExtension
        {
            get => $"{Config.ImageExportFileFormat}";
        }
    }
}
