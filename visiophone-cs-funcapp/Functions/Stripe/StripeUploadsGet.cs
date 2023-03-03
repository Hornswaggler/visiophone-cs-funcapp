using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using vp.models;
using Stripe;
using System;
using System.Linq;
using vp.functions.samplepack;

namespace vp.functions.stripe
{
    public class StripeUploadsGet : AuthStripeBase
    {
        private readonly ISamplePackService _samplePackService;

        public StripeUploadsGet(
            IUserService userService, 
            IStripeService stripeService, 
            IValidationService validationService,
            ISamplePackService samplePackService
        )
            : base(userService, stripeService, validationService)
        {
            _samplePackService = samplePackService;
        }

        [FunctionName(FunctionNames.StripeUploadsGet)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            StripeProfileResult account;
            try
            {
                account = await AuthorizeStripeUser(req);
            }
            catch (UnauthorizedAccessException e)
            {
                log.LogWarning("Unauthorized samplepack upload", e);
                return new UnauthorizedResult();
            }

            try
            {
                //var stripeProfile = await _stripeService.GetStripeProfile(_userService.GetUserAccountId(req.HttpContext.User), true);
                List<Sample> result = new List<Sample>();

                if (account.isStripeApproved)
                {
                    SearchQueryResult<SamplePack<Sample>> samplePacks = await _samplePackService.GetSamplePacksBySellerId(new SearchQueryRequest
                    {
                        index = 0,
                        query = account.stripeId
                    });

                    return new OkObjectResult(samplePacks);
                }

                return new OkObjectResult(result);
            }
            catch
            {
                //consume
            }

            return new OkObjectResult(new StripeProfileResult());
        }
    }
}
