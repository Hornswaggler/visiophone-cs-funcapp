using System.Collections.Generic;

namespace vp.orchestrations
{
    public class UpsertSamplePackRequest
    {
        public string name { get; set; }
        public string description { get; set; }

        public List<UpsertSampleRequest> sampleRequests;
    }
}
