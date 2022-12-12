namespace vp.models
{
    public class CheckoutSession : BaseModel
    {
        public CheckoutSession() : base() { }

        public string stripeSessionId {get;set;}
        public string accountId { get; set; }
    }
}
