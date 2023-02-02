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

namespace vp.functions.sample
{
    public class SampleSearch : AuthBase
    {
        protected readonly ISampleService _sampleService;

        public SampleSearch(IUserService userService, ISampleService sampleService) 
            : base(userService) 
        {
            _sampleService = sampleService;
        }

        [FunctionName(FunctionNames.SampleSearch)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await AuthorizeUser(req)) {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SearchQuery request = JsonConvert.DeserializeObject<SearchQuery>(requestBody);

            return new OkObjectResult(await _sampleService.GetSamplesByName(request));
        }
    }
}
