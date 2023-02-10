using Stripe;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackTransaction
    {
        public Account account { get; set; }
        public string userName { get; set; }
        public UpsertSamplePackRequest request { get; set; }
    }
}
