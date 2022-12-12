using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using vp.services;

namespace vp.Functions.User
{
    public class ProvisionStripeStandardReturn
    {
        private IUserService _userService { get; set; }
        private IStripeService _stripeService { get; set; }

        public ProvisionStripeStandardReturn(IUserService userService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
        }

        internal class ProvisionSellerAccountDTO
        {
            public string stripeId { get; set; }
        }

        [FunctionName("provision_stripe_standard_return")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = null)] HttpRequest req,
            ILogger log, ClaimsPrincipal principal
        )
        {
            if (!await _userService.AuthenticateUser(req, log))
            {
                return new UnauthorizedResult();
            }

            var stripeProfile = _stripeService.GetStripeProfile(_userService.GetUserAccountId(req.HttpContext.User));

            //TODO: This should come from the token...
            var stripeAccount = await _stripeService.GetStripeAccount(stripeProfile);

            if (stripeAccount.DetailsSubmitted)
            {
                stripeProfile.isStripeApproved = true;
                stripeProfile = await _stripeService.SetStripeProfile(stripeProfile);
            }

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return new OkObjectResult(stripeProfile);
        }
    }
}
