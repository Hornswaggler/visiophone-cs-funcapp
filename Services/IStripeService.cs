using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public interface IStripeService
    {
        Task<Stripe.Account> GetStripeAccount(StripeProfile stripeProfile);
        Task<StripeProfile> CreateNewAccount(string accountId);
        StripeProfile GetStripeProfile(string accountId, bool throwNoExist = false);

        Task<StripeProfile> SetStripeProfile(StripeProfile stripeProfile);
        Task<Stripe.AccountLink> CreateAccountLink(string stripeId);
        Stripe.Checkout.Session CreateSession(SamplePurchaseRequest purchaseRequest);
    }
}
