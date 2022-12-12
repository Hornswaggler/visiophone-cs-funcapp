
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using vp.services;

namespace vp.Functions.User
{
    public class ProvisionStripeStandard
    {
        private IStripeService _stripeService { get; set; }
        private IUserService _userService { get; set; }

        public ProvisionStripeStandard(IStripeService stripeService, IUserService userService)
        {
            _stripeService = stripeService;
            _userService = userService;
        }

        [FunctionName("provision_stripe_standard")]
        public async Task<IActionResult> Run(
            [Microsoft.Azure.WebJobs.HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await _userService.AuthenticateUser(req, log))
            {
                return new UnauthorizedResult();
            }

            string accountId = _userService.GetUserAccountId(req.HttpContext.User);
            var stripeProfile = _stripeService.GetStripeProfile(accountId);
            if(stripeProfile == null)
            {
                stripeProfile = await _stripeService.CreateNewAccount(accountId);
            }

            var accountLink = await _stripeService.CreateAccountLink(stripeProfile.stripeId);
            stripeProfile.stripeUri = accountLink.Url;

            req.HttpContext.Response.Headers.Add("Location", accountLink.Url);
            return new StatusCodeResult(303);
        }
    }
}
