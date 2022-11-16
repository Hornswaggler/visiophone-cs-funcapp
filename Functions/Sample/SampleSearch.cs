using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using vp.services;
using vp.DTO;

namespace vp.Functions.Sample
{
    public class SampleSearch 
    {
        private readonly ISampleService _sampleService;

        public SampleSearch(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [FunctionName("sample_search")]
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
