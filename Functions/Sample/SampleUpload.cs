using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using vp.orchestrations;
using vp.services;

namespace vp.functions.sample
{
    public class SampleUploadFunction : SampleFunctionBase
    {
        public SampleUploadFunction(IUserService userService, ISampleService sampleService)
            : base(userService, sampleService) { }

        [FunctionName("sample_upload")]
        public async Task<IActionResult> SampleUpload (
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var userName = req.HttpContext.User.FindFirst("name")?.Value ?? "";

            Account account;
            try
            {
                account = AuthorizeStripeUser(req, log);
            }
            catch (UnauthorizedAccessException e)
            {
                log.LogWarning($"Unauthorized sample upload: {userName} , {e.Message}", e);
                return new UnauthorizedResult();
            }

            UpsertSampleRequest upsertSampleRequest = null;
            try
            {
                var formData = req.Form["data"];
                upsertSampleRequest = JsonConvert.DeserializeObject<UpsertSampleRequest>(formData);
                upsertSampleRequest.seller = userName;
                upsertSampleRequest.sellerId = account.Id;
            }
            catch (Exception e)
            {
                //TODO: Rollback the transaction
                log.LogError($"Sample deserialization failed: {e.Message}", e);
                return new BadRequestResult();
            }

            var form = req.Form;
            //TODO: These data uploads need to be deleted if / when the transaction fails
            try
            {
                util.Utils.UploadFormFile(
                    form.Files[upsertSampleRequest.sampleFileName], 
                    Config.SampleBlobContainerName, 
                    upsertSampleRequest.sampleFileName);
            }
            catch (Exception e)
            {
                //TODO: Rollback the upload(s)
                log.LogError($"Sample upload failed {e.Message}", e);
                return new BadRequestResult();
            }

            //TODO: What if anything should we do w/ this orchestration Id?
            var transactionMetadata = new UpsertSampleTransaction(account, upsertSampleRequest);
            var orchestrationId = await starter.StartNewAsync<UpsertSampleTransaction>(
                OrchestratorNames.UpsertSample,
                transactionMetadata
            );

            return new OkObjectResult(upsertSampleRequest);
        }
    }
}
