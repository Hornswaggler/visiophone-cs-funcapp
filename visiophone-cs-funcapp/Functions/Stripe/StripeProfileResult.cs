using System.Collections.Generic;
using vp.models;

namespace vp.functions.stripe
{
    public class StripeProfileResult : StripeProfile
    {
        public List<Sample> uploads { get; set; }
        public string defaultCurrency { get; set; }
        public bool isStripeApproved { get; set; } = false;

        public StripeProfileResult() : base()
        {

        }

        public StripeProfileResult(StripeProfile profile)
        {
            accountId = profile.accountId;
            stripeId = profile.stripeId;
        }
    }
}
