using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading.Tasks;
using vp.models;
using vp.services;

namespace vp.webhooks
{
    public static class CheckoutSessionCompleted
    {
        public static async Task<IActionResult> HandleEvent(
            EventData eventData,
            IStripeService stripeService,
            IPurchaseService purchaseService
        )
        {
            //TODO: THis should be in an orchestration in case it fails, it can be rerun!!!
            var stripeSession = eventData.Object as Stripe.Checkout.Session;

            var session = stripeService.GetCheckoutSession(stripeSession.Id);
            var priceIds = stripeService.GetPriceIdsForSession(stripeSession.Id);

            foreach (var priceId in priceIds)
            {
                await purchaseService.AddPurchase(new Purchase
                {
                    accountId = session.Metadata["vp_accountId"],
                    priceId = priceId,
                    type = "samplePacks"
                });
            }
           
            return new OkResult();
        }
    }
}
