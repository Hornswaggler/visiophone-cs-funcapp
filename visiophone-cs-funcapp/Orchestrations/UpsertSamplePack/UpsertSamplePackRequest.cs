using System.Collections.Generic;
using vp.models;
using vp.orchestrations.upsertsample;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackRequest : SamplePack<UpsertSampleRequest>
    {
        public string imgUrl { get; set; }
        public string stagingImgBlobPath { get; set; }
    }
}
