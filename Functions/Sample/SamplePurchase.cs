
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using vp.services;
using Microsoft.Extensions.Logging;
using vp.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using vp.Services;
using Newtonsoft.Json;

namespace visiophone_cs_funcapp.Functions.Sample
{
    public class SamplePurchase
    {
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;
        private readonly ICheckoutSessionService _checkoutSessionService;


        public SamplePurchase(IUserService userService, ICheckoutSessionService checkoutSessionService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
            _checkoutSessionService = checkoutSessionService;
        }

        [FunctionName("sample_purchase")]
        public  IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!_userService.AuthenticateUserForm(req, log))
            {
                return new UnauthorizedResult();

            }

            var prices = req.Form["prices"].ToString();
            var priceIds = JsonConvert.DeserializeObject<List<string>>(prices);

            var samples = new List<SampleDTO>();
            foreach(var priceId in priceIds)
            {
                samples.Add(new SampleDTO { priceId = priceId });
            }

            var samplePurchaseRequest = new SamplePurchaseRequest
            {
                samples = samples
            };

            Stripe.Checkout.Session session = _stripeService.CreateSession(samplePurchaseRequest);
            return new RedirectResult(session.Url);
        }
    }
}
