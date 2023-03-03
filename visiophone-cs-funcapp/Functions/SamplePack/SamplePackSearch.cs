using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using vp.services;

namespace vp.functions.samplepack{
    public class SamplePackSearch : AuthBase
    {
        ISamplePackService _samplePackService;
        public SamplePackSearch(IUserService userService, ISamplePackService samplePackService, IValidationService validationService) 
            : base(userService, validationService)
        { 
            _samplePackService = samplePackService;
        }

        [FunctionName(FunctionNames.SamplePackSearch)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            if (!await _userService.AuthenticateUser(req))
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SearchQueryRequest request = JsonConvert.DeserializeObject<SearchQueryRequest>(requestBody);

            return new OkObjectResult(await _samplePackService.GetSamplePacksByName(request));
        }
    }
}
