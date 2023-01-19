
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using vp.services;

namespace vp.functions.User
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

        [FunctionName("account_upgrade")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string accountId = null;
            try
            {
                accountId = _userService.AuthenticateUserForm(req, log);
            }
            catch {
                return new UnauthorizedResult();
            }

            var stripeProfile = _stripeService.GetStripeProfile(accountId);
            if(stripeProfile == null)
            {
                stripeProfile = await _stripeService.CreateNewAccount(accountId);
            }

            var accountLink = await _stripeService.CreateAccountLink(stripeProfile.stripeId);
            req.HttpContext.Response.Headers.Add("Location", accountLink.Url);
            return new StatusCodeResult(303);
        }
    }
}
