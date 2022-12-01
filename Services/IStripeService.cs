using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public interface IStripeService
    {
        Task<Stripe.Account> GetStripeAccount(StripeProfile stripeProfile);
        Task<string> CreateNewAccount(string accountId);
        StripeProfile GetStripeProfile(string accountId, bool throwNoExist = false);

        Task<StripeProfile> SetStripeProfile(StripeProfile stripeProfile);

    }
}
