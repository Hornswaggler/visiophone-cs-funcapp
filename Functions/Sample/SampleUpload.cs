using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.services;
using vp.util;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace vp.Functions.Sample
{
    public class SampleUpload
    {
        private readonly ISampleService _sampleService;
        private readonly IUserService _userService;
        private readonly ILogger<SampleUpload> _log;
        private readonly IStripeService _stripeService;

        public SampleUpload(ISampleService sampleService, IUserService userService, IStripeService stripeService, ILogger<SampleUpload> log)
        {
            _sampleService = sampleService;
            _userService = userService;
            _stripeService = stripeService;
            _log = log;
        }

        [FunctionName("sample_upload")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {
            var stripeAccount = _userService.AuthenticateSeller(req, log);
            if(stripeAccount.Result == null)
            {
                return new UnauthorizedResult();
            }

            //TODO: Transactionalize this!
            var sampleMetadata = JsonConvert.DeserializeObject<models.Sample>(req.Form["data"]);
            var account = stripeAccount.Result as Stripe.Account;

            var options = new Stripe.ProductCreateOptions
            {
                Name = sampleMetadata.name,
                Description = sampleMetadata.description,
                DefaultPriceData = new Stripe.ProductDefaultPriceDataOptions {
                    Currency = "USD",
                    UnitAmountDecimal = sampleMetadata.cost
                },
                Metadata = new Dictionary<string, string>
                {
                    { "accountId", $"{account.Id}" },
                }
            };

            var service = new Stripe.ProductService();
            var stripeProduct = service.Create(options);
            sampleMetadata.priceId = stripeProduct.DefaultPriceId;
            sampleMetadata.sellerId = account.Id;

            await _sampleService.AddSample(sampleMetadata);

            var form = req.Form;
            string filename = $"{sampleMetadata._id}";

            var sample = form.Files["sample"];
            Utils.UploadFormFileAsync(sample, Config.SampleBlobContainerName, filename);

            var image = form.Files["image"];
            Utils.UploadFormFileAsync(image, Config.CoverArtContainerName, filename);

            return new OkObjectResult(sampleMetadata);
        }
    }
}
