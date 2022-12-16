using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.services;

namespace visiophone_cs_funcapp.Functions.Sample
{
    public class GetPurchases
    {
        private readonly ISampleService _sampleService;
        private readonly IUserService _userService;

        public GetPurchases(ISampleService sampleService, IUserService userService)
        {
            _sampleService = sampleService;
            _userService = userService;
        }

        [FunctionName("get_purchases")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await _userService.AuthenticateUser(req, log))
            {
                return new UnauthorizedResult();
            }
            var accountId = _userService.GetUserAccountId(req.HttpContext.User);
            var purchases =  await _sampleService.GetPurchases(accountId);

            List<string> priceIds = new List<string>();
            foreach (var purchase in purchases)
            {
                priceIds.Add(purchase.priceId);
            }

            return new OkObjectResult(await _sampleService.GetSamples(priceIds));
        }
    }
}
