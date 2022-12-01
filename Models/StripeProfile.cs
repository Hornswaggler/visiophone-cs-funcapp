

namespace vp.models
{
    public class StripeProfile : BaseModel
    {
        public string accountId { get; set; } = "";
        public string stripeId { get; set; } = "";
        public bool isStripeApproved { get; set; } = false;
    }
}
