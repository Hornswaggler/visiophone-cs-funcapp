using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using vp.services;

namespace vp.functions.sample
{
    public class SampleBase
    {
        protected readonly IUserService _userService;
        protected readonly ISampleService _sampleService;

        public SampleBase(IUserService userService, ISampleService sampleService)
        {
            _userService = userService;
            _sampleService = sampleService;
        }

        protected Account AuthorizeStripeUser(HttpRequest req, ILogger log)
        {
            var stripeAccount = _userService.AuthenticateSeller(req, log);

            if (stripeAccount.Result == null) throw new UnauthorizedAccessException();

            return stripeAccount.Result;
        }
    } 
}
