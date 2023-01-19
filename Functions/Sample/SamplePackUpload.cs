
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Threading.Tasks;
using vp.orchestrations;
using vp.orchestrations.upsertSamplePack;
using vp.services;

namespace vp.functions.sample
{
    public class SamplePackUploadFunction : SampleFunctionBase
    {
        public SamplePackUploadFunction(IUserService userService, ISampleService sampleService) 
            :base(userService, sampleService) { }

        [FunctionName("sample_pack_upload")]
        public async Task<IActionResult> SamplePackUpload (
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            Account account;
            try
            {
                account = AuthorizeStripeUser(req, log);
            }
            catch (UnauthorizedAccessException e)
            {
                log.LogWarning("Unauthorized samplepack upload", e);
                return new UnauthorizedResult();
            }

            var userName = req.HttpContext.User.FindFirst("name")?.Value ?? "";

            UpsertSamplePackTransaction upsertSamplePackRequest;
            try
            {
                var formData = req.Form["data"];
                upsertSamplePackRequest = JsonConvert.DeserializeObject<UpsertSamplePackTransaction>(formData);
                upsertSamplePackRequest.account = account;
                upsertSamplePackRequest.userName = userName;
            }
            catch (Exception e)
            {
                //TODO: Rollback the transaction
                log.LogError($"Samplepack deserialization failed: {e.Message}", e);
                return new BadRequestResult();
            }

            var form = req.Form;
            try
            {
                foreach(var sampleRequest in upsertSamplePackRequest.sampleRequests)
                {
                    util.Utils.UploadFormFile(
                        form.Files[sampleRequest.sampleFileName],
                        Config.SampleBlobContainerName,
                        sampleRequest.sampleFileName);
                }
            }
            catch (Exception e)
            {
                //TODO: Rollback the upload(s)
                log.LogError($"Samplepack sample uploads failed {e.Message}", e);
                return new BadRequestResult();
            }

            try
            {
                foreach (var sampleRequest in upsertSamplePackRequest.sampleRequests)
                {
                    util.Utils.UploadFormFile(
                        form.Files[sampleRequest.imageFileName],
                        Config.SampleBlobContainerName,
                        sampleRequest.imageFileName);
                }
            }
            catch (Exception e)
            {
                //TODO: Rollback the upload(s)
                log.LogError($"Samplepack image uploads failed {e.Message}", e);
                return new BadRequestResult();
            }


            //TODO: What if anything should we do w/ this orchestration Id?
            var orchestrationId = await starter.StartNewAsync(
                OrchestratorNames.UpsertSamplePack,
                upsertSamplePackRequest
            );

            return new OkObjectResult(orchestrationId);
        }

    }
}
