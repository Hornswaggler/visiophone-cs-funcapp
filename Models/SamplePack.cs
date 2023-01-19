
using System.Collections.Generic;
using vp.orchestrations;

namespace vp.models
{
    public class SamplePack : BaseModel
    {
        public SamplePack() : base() { }
        public List<UpsertSampleRequest> samples { get; set; } = new List<UpsertSampleRequest>();
        public string name { get; set; }
        public string description { get; set; }
    }
}
