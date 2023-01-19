using vp.models;

namespace vp.orchestrations
{
    public class UpsertSampleRequest
    {
        public string requestId { get; set; }
        public Sample sampleMetadata { get; set; }

        public string sampleFileName { get; set; }
        public string imageFileName { get; set; }
    }
}
