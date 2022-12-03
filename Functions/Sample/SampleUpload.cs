using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.services;
using vp.util;
using Microsoft.AspNetCore.Mvc;
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
            if(stripeAccount == null)
            {
                return new UnauthorizedResult();
            }

            //TODO: Connect to stripe, provision the product in the product database there...

            //TODO: AUTHORIZE THE USER (CHECK ACCOUNT) interface Stripe etc

            models.Sample sampleMetadata = new models.Sample();
            sampleMetadata = JsonConvert.DeserializeObject<models.Sample>(req.Form["data"]);



            //await _sampleService.AddSample(sampleMetadata);
            //await _userService.AddForSale(req.Form["accountId"], sampleMetadata._id );
                 
            //var form = req.Form;
            //string filename = $"{sampleMetadata._id}";

            //var sample = form.Files["sample"];
            //Utils.UploadFormFileAsync(sample, Config.SampleBlobContainerName, filename);

            //var image = form.Files["image"];
            //Utils.UploadFormFileAsync(image, Config.CoverArtContainerName, filename);

            return new OkObjectResult(sampleMetadata);
        }
    }
}
