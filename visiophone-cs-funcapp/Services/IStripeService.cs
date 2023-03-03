using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.functions.stripe;
using vp.models;

namespace vp.services
{
    public interface IStripeService
    {
        Task<Account> GetStripeAccount(StripeProfile stripeProfile);
        Task<StripeProfile> CreateNewAccount(string accountId);
        Task<StripeProfileResult> GetStripeProfile(string accountId, bool throwNoExist = false);
        Task<AccountLink> CreateAccountLink(string stripeId, string returnUri);
        Session CreateSession(string accountId, List<string> priceIds);
        List<Product> GetProductsForUser(string stripeId);
        Session GetCheckoutSession(string sessionId);
        List<string> GetPriceIdsForSession(string sessionId);
        Task<string> GetAccountLink(StripeProfile profile, string returnUri);
    }
}
