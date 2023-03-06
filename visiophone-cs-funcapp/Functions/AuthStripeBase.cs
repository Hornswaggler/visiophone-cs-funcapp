using Microsoft.AspNetCore.Http;
using Stripe;
using System.Threading.Tasks;
using vp.functions.stripe;
using vp.services;

namespace vp.functions
{
    public class AuthStripeBase : AuthBase
    {
        protected readonly IStripeService _stripeService;

        public AuthStripeBase(
            IUserService userService, 
            IStripeService stripeService, 
            IValidationService validationService
        ) 
            : base(userService, validationService)
        {
            _stripeService = stripeService;
        }
        protected async Task<StripeProfileResult> AuthorizeStripeUser(HttpRequest req)
        {
            if (await AuthorizeUser(req))
            {
                var userId = _userService.GetUserAccountId(req.HttpContext.User);

                var stripeProfile = await _stripeService.GetStripeProfile(userId);

                return stripeProfile;
            }

            return null;

        }
    }
}
