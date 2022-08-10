using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.models;
using vp.services;
using System.Collections.Generic;
using vp.DTO;

namespace vp.functions
{
    public class Sample
    {
        private readonly ISampleService _sampleService;

        public Sample(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [FunctionName("sample")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SampleRequest request = JsonConvert.DeserializeObject<SampleRequest>(requestBody);

            return new OkObjectResult(await _sampleService.GetSamples(request));
        }
    }
}
