using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using System.IO;
using System.Threading.Tasks;
using vp.models;
using vp.services;

namespace vp.functions.sample
{
    public class CheckoutSessionCompletedWebhook
    {
        private readonly ISampleService _sampleService;
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;
        public CheckoutSessionCompletedWebhook(ISampleService sampleService, IUserService userService, IStripeService stripeService)
        {
            _sampleService = sampleService;
            _userService = userService;
            _stripeService = stripeService;
        }

        [FunctionName("handle_checkout_session_completed")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(requestBody, req.HttpContext.Request.Headers["Stripe-Signature"], Config.CheckoutSessionCompletedSecret);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var stripeEventData = stripeEvent.Data;
                    var stripeSession = stripeEventData.Object as Stripe.Checkout.Session;

                    var session = _stripeService.GetCheckoutSession(stripeSession.Id);
                    var priceIds = _stripeService.GetPriceIdsForSession(stripeSession.Id);

                    foreach (var priceId in priceIds)
                    {
                        await _sampleService.AddPurchase(new Purchase
                        {
                            accountId = session.Metadata["vp_accountId"],
                            priceId = priceId,
                        });
                    }
                }
            } catch  {
                return new UnauthorizedResult();
            }

            return new OkResult();
        }
    }
}
