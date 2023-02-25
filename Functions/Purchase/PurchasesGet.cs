using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.services;

namespace vp.functions.purchase
{
    public class PurchaseGet : AuthBase
    {
        private readonly ISamplePackService _samplePackService;
        private readonly IPurchaseService _purchaseService;

        public PurchaseGet(
            IUserService userService,
            ISamplePackService samplePackService,
            IPurchaseService purchaseService,
            IValidationService validationService
        ) : base(userService, validationService)
        {
            _samplePackService = samplePackService;
            _purchaseService = purchaseService;
        }

        [FunctionName(FunctionNames.PurchaseGet)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            if (!await AuthorizeUser(req))
            {
                return new UnauthorizedResult();
            }

            var accountId = _userService.GetUserAccountId(req.HttpContext.User);
            var purchases = await _purchaseService.GetPurchases(accountId);

            List<string> priceIds = new List<string>();
            foreach (var purchase in purchases)
            {
                priceIds.Add(purchase.priceId);
            }

            return new OkObjectResult(await _samplePackService.GetSamplePackPurchasesByPriceIds(priceIds));
        }
    }
}