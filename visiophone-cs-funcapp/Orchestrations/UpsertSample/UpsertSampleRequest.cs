using vp.models;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleRequest : Sample
    {
        public string clipUri { get; set; }
        public string stagingBlobPath { get; set; } = "";

        public string fileExtension { get; set; }
    }
}
