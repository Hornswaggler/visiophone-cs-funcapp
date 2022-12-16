using System.Collections.Generic;
using vp.models;

namespace vp.DTO
{
    public class StripeProfileDTO : StripeProfile
    {
        public List<Sample> uploads { get; set; }

        public StripeProfileDTO() : base()
        {

        }

        public StripeProfileDTO(StripeProfile profile)
        {
            accountId = profile.accountId;
            stripeId = profile.stripeId;
            isStripeApproved = profile.isStripeApproved;
        }
    }
}
