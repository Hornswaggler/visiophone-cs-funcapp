using vp.models;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleRequest : Sample
    {
        public string sampleFileName { get; set; }
        public string fileExtension { get; set; }
    }
}
