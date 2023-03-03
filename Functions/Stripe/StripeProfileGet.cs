using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace vp.functions.stripe
{
    public class StripeProfileGet
    {
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;

        public StripeProfileGet(IUserService userService, IStripeService stripeService)
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
                var result = await _stripeService.GetStripeProfile(_userService.GetUserAccountId(req.HttpContext.User), true);
                return new OkObjectResult(JsonConvert.SerializeObject(result));
            }
            catch
            {
                //consume
            }

            return new OkObjectResult(new StripeProfileResult());
        }
    }
}
