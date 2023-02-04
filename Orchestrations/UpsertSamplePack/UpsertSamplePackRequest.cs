using System.Collections.Generic;
using vp.orchestrations.upsertsample;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackRequest
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public string imageFileName { get; set; }

        public List<UpsertSampleRequest> sampleRequests;
    }
}
