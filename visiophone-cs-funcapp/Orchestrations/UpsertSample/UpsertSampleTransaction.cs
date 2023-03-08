using vp.functions.stripe;

namespace vp.orchestrations.upsertsample
{ 
    public class UpsertSampleTransaction 
    {
        //TODO: Security risk, take out what is needed...
        public StripeProfileResult account { get; set; }
        public UpsertSampleRequest request { get; set; }
        public string samplePackId { get; set; }

        public UpsertSampleTransaction(StripeProfileResult _account, UpsertSampleRequest _sampleMetadata, string samplePackId)
        {
            account = _account;
            request = _sampleMetadata;
            this.samplePackId = samplePackId;
        }
    }
}
