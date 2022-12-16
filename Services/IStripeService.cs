using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        Session CreateSession(string accountId, List<string> priceIds);
        List<Sample> GetProductsForUser(string stripeId);
        Session GetCheckoutSession(string sessionId);
        List<string> GetPriceIdsForSession(string sessionId);
    }
}
