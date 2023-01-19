using Stripe;
using System.Collections.Generic;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackTransaction : TransactionBase
    {
        public Account account { get; set; }
        public string userName { get; set; }

        public List<UpsertSampleRequest> sampleRequests { get; set; } = new List<UpsertSampleRequest>();
    }
}
