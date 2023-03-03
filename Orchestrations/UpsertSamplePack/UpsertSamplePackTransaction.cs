using vp.functions.stripe;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackTransaction
    {
        public StripeProfileResult account { get; set; }
        public string userName { get; set; }
        public UpsertSamplePackRequest request { get; set; }
    }
}
