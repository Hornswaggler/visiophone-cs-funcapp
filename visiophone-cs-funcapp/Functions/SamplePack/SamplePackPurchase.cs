
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using vp.services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using vp.functions;

namespace visiophone_cs_funcapp.Functions.SamplePack
{
    public class SamplePackPurchase
    {
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;

        public SamplePackPurchase(IUserService userService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
        }

        [FunctionName(FunctionNames.SamplePackPurchase)]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = null)] HttpRequest req,
            ILogger log)
        {
            string accountId = "";
            try
            {
                accountId = _userService.AuthenticateUserForm(req, log);
            }
            catch
            {
                return new UnauthorizedResult();
            }

            var priceIds = JsonConvert.DeserializeObject<List<string>>(req.Form["payload"].ToString());
            Stripe.Checkout.Session session = _stripeService.CreateSession(accountId, priceIds);

            return new RedirectResult(session.Url);
        }
    }
}
