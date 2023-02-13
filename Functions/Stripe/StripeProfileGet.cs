using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using Microsoft.AspNetCore.Mvc;
using vp.DTO;

namespace vp.functions.stripe
{
    public class StripeProfileGet
    {
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;

        public StripeProfileGet(IUserService userService, ISampleService sampleService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
        }

        [FunctionName(FunctionNames.StripeProfileGet)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            if (!await _userService.AuthenticateUser(req))
            {
                return new UnauthorizedResult();
            }

            try
            {
                //TODO: Security vulnerability, no reason to pass forward account id or stripe id...
                var profile = await _stripeService.GetStripeProfile(_userService.GetUserAccountId(req.HttpContext.User), true);
                StripeProfileDTO result = new StripeProfileDTO
                {
                    accountId = profile.accountId,
                    stripeId = profile.stripeId,
                    isStripeApproved = profile.isStripeApproved,
                };
                
                if (result.isStripeApproved)
                {
                    result.uploads = _stripeService.GetProductsForUser(profile.stripeId);
                }

                return new OkObjectResult(result);
            }
            catch
            {
                //consume
            }

            return new OkObjectResult(new StripeProfileDTO());
        }
    }
}
