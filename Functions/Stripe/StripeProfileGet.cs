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
                var profile = _stripeService.GetStripeProfile(_userService.GetUserAccountId(req.HttpContext.User), true);
                var result = new StripeProfileDTO(profile);

                if (profile.isStripeApproved)
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
