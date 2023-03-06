using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.IO;
using System.Threading.Tasks;
using vp.services;

namespace vp.webhooks
{
    public class StripeWebhookHandler
    {
        private readonly IStripeService _stripeService;
        private readonly IPurchaseService _purchaseService;

        public StripeWebhookHandler(
            IStripeService stripeService,
            IPurchaseService purchaseService)
        {
            _stripeService = stripeService;
            _purchaseService = purchaseService;
        }

        [FunctionName(WebhookNames.WebhookFunctionName)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(
                    requestBody,
                    req.HttpContext.Request.Headers["Stripe-Signature"],
                    Config.StripeWebhookSigningSecret
                );

                var eventType = stripeEvent.Type;
                var eventData = stripeEvent.Data;

                switch (stripeEvent.Type)
                {
                    case WebhookNames.CheckoutSessionCompleted:
                        return await CheckoutSessionCompleted.HandleEvent(eventData, _stripeService, _purchaseService);
                    default:
                        return new OkResult();
                }

            }
            catch (Exception e)
            {
                log.LogError($"Webhook failed: {e.Message}", e);
                return new BadRequestResult();
            }
        }
    }
}
