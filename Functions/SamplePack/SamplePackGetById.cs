using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using vp.services;

namespace vp.functions.samplepack
{
    public class SamplePackGetById : AuthBase
    {
        ISamplePackService _samplePackService;

        public SamplePackGetById(
            IUserService userService,
            ISamplePackService samplePackService,
            IValidationService validationService
        ) : base(userService, validationService)
        {
            _samplePackService = samplePackService;
        }

        [FunctionName(FunctionNames.SamplePackGetById)]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = $"{FunctionNames.SamplePackGetById}/{{id}}")] HttpRequest req,
                string? id
        )
        {
            if (!await _userService.AuthenticateUser(req))
            {
                return new UnauthorizedResult();
            }

            var result = await _samplePackService.GetSamplePackById(id);
            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }
    }
}
