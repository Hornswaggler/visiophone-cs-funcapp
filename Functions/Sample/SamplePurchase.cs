
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using vp.services;
using Microsoft.Extensions.Logging;
using System.IO;
using vp.DTO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using vp.Models;

namespace visiophone_cs_funcapp.Functions.Sample
{
    public class SamplePurchase
    {
        private readonly IUserService _userService;
        private readonly ISampleService _sampleService;

        public SamplePurchase(IUserService userService, ISampleService sampleService)
        {
            _userService = userService;
            _sampleService = sampleService;
        }

        [FunctionName("sample_purchase")]
        public async Task<UserProfileModel> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ClaimsPrincipal principal,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<IdDTO>(requestBody);

            log.LogInformation($"Sample Purchase: {request.accountId} : {request._id}");

            if (_userService.isAuthenticated(principal, request.accountId)) {
                var sample = await _sampleService.GetSampleById(request._id);
                if (sample != null)
                {
                    //TODO: Make sure user doesn't already own this sample...
                    return await _userService.PurchaseSample(request.accountId, sample._id);
                }
            }

            var error = $"Unauthorized Sample Purchase: {request.accountId} : {request._id}";
            var e = new Exception(error);
            log.LogCritical(e, error);
            throw e;
        }
    }
}
