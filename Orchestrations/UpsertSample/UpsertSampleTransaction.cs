using Stripe;

namespace vp.orchestrations
{
    public class UpsertSampleTransaction 
    {
        //TODO: Security risk, take out what is needed...
        public Account account { get; set; }
        public UpsertSampleRequest request { get; set; }

        public UpsertSampleTransaction(Account _account, UpsertSampleRequest _sampleMetadata)
        {
            account = _account;
            request = _sampleMetadata;
        }
    }
}
