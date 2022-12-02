using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using vp.models;
using vp.services;

namespace vp.Functions.User
{
    public class ProvisionSellerAccount
    {
        private IUserService _userService { get; set; }
        private IStripeService _stripeService { get; set; }

        public ProvisionSellerAccount(IUserService userService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
        }

        internal class ProvisionSellerAccountDTO
        {
            public string stripeId { get; set; }
        }


        [FunctionName("provision_stripe_standard")]
        public async Task<IActionResult> ProvisionStripeStandard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var (authenticationStatus, authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();

            if (!authenticationStatus) return authenticationResponse;

            var user = req.HttpContext.User;

            if (!user.HasClaim("tfp", "B2C_1_SIGN_IN"))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var accountId = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            StripeProfile stripeProfile = _stripeService.GetStripeProfile(accountId);

            if (stripeProfile != null) {
                return new OkObjectResult(stripeProfile);
            }

            StripeProfile newStripeProfile = await _stripeService.CreateNewAccount(accountId);
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            //TODO This should be a redirect
            return new OkObjectResult(newStripeProfile);
        }

        [FunctionName("provision_stripe_standard_return")]
        public async Task<IActionResult> ProvisionStripeStandardReturn(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = null)] HttpRequest req,
            ILogger log, ClaimsPrincipal principal
        )
        {
            if (!await _userService.AuthenticateUser(req, log))
            {
                return new UnauthorizedResult();
            }

            var stripeProfile = _stripeService.GetStripeProfile(_userService.GetUserAccountId(req));

            //TODO: This should come from the token...
            var stripeAccount = await _stripeService.GetStripeAccount(stripeProfile);

            //Persist stripeId
            if (stripeAccount.DetailsSubmitted)
            {
                stripeProfile.isStripeApproved = true;
                stripeProfile = await _stripeService.SetStripeProfile(stripeProfile);
            }

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return new OkObjectResult(stripeProfile);
        }

        [FunctionName("provision_stripe_standard_refresh")]
        public string ProvisionStripeStandardRefresh(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log
        )
        {
            //log.LogInformation("Processing string standard response");
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //var request = JsonConvert.DeserializeObject<ProvisionSellerAccountDTO>(requestBody);


            return null;
        }
    }
}
