
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

namespace vp.functions.samplePack
{
    public class SamplePackUpload : AuthBase
    {
        public SamplePackUpload(IUserService userService) : base(userService) { }

        [FunctionName(FunctionNames.SamplePackUpload)]
        public async Task<IActionResult> Run (
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            Account account;
            try
            {
                account = AuthorizeStripeUser(req);
            }
            catch (UnauthorizedAccessException e)
            {
                log.LogWarning("Unauthorized samplepack upload", e);
                return new UnauthorizedResult();
            }

            var userName = req.HttpContext.User.FindFirst("name")?.Value ?? "";

            UpsertSamplePackTransaction transaction;
            try
            {
                var formData = req.Form["data"];
                var upsertSamplePackRequest = JsonConvert.DeserializeObject<UpsertSamplePackRequest>(formData);

                transaction = new UpsertSamplePackTransaction
                {
                    account = account,
                    userName = userName,
                    request = upsertSamplePackRequest
                };

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
                var sampleRequests = transaction.request.sampleRequests;
                foreach (var sampleRequest in sampleRequests)
                {
                    vp.util.Utils.UploadFormFile(
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

            //UPLOAD THE IMAGE for the pack
            //try
            //{
            //    util.Utils.UploadFormFile(
            //        form.Files[upsertSampleRequest.imageFileName], 
            //        Config.CoverArtContainerName, 
            //        upsertSampleRequest.imageFileName);
            //}
            //catch (Exception e)
            //{
            //    //TODO: Rollback the upload(s)
            //    log.LogError($"Image upload failed {e.Message}", e);
            //    return new BadRequestResult();
            //}


            //TODO: What if anything should we do w/ this orchestration Id?
            var orchestrationId = await starter.StartNewAsync(
                OrchestratorNames.UpsertSamplePack,
                transaction
            );

            return new OkObjectResult(orchestrationId);
        }

    }
}
