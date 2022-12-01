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
        private readonly IUserService _userService;

        public SampleSearch(ISampleService sampleService, IUserService userService)
        {
            _sampleService = sampleService;
            _userService = userService;
        }

        [FunctionName("sample_search")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await _userService.AuthenticateUser(req, log)) {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SampleRequest request = JsonConvert.DeserializeObject<SampleRequest>(requestBody);

            return new OkObjectResult(await _sampleService.GetSamples(request));
        }
    }
}
