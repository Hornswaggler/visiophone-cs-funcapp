using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Threading.Tasks;
using vp.services;

namespace vp.functions
{
    public class AuthBase
    {
        protected readonly IUserService _userService;
        protected readonly IValidationService _validationService;
        protected readonly ILogger _logger;

        public AuthBase(IUserService userService, IValidationService validationService)
        {
            _userService = userService;
            _validationService = validationService;
        }

        protected async Task<bool> AuthorizeUser(HttpRequest req) {
            return await _userService.AuthenticateUser(req);
        }

        protected Account AuthorizeStripeUser(HttpRequest req)
        {
            var stripeAccount = _userService.AuthenticateSeller(req, _logger);
            if (stripeAccount.Result == null) throw new UnauthorizedAccessException();
            return stripeAccount.Result;
        }
    }
}
