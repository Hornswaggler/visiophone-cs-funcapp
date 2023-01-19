using Stripe;
using vp.orchestrations.upsertsample;

namespace vp.orchestrations
{
    public class UpsertSampleTransaction 
    {
        public UpsertSampleRequest request { get; set; }
        public Account account { get; set; }

        public UpsertSampleTransaction(Account _account, UpsertSampleRequest _sampleMetadata)
            : base()
        {
            account = _account;
            request = _sampleMetadata;
        }
    }
}
