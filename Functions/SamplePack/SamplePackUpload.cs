
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Threading.Tasks;
using vp.orchestrations;
using vp.orchestrations.upsertSamplePack;
using vp.services;
using vp.util;

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
            UpsertSamplePackRequest samplePackRequest;
            try
            {
                var formData = req.Form["data"];
                samplePackRequest = JsonConvert.DeserializeObject<UpsertSamplePackRequest>(formData);
                samplePackRequest._id = ObjectId.GenerateNewId().ToString();

                transaction = new UpsertSamplePackTransaction
                {
                    account = account,
                    userName = userName,
                    request = samplePackRequest
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
                    sampleRequest._id = ObjectId.GenerateNewId().ToString();

                    var ext = Utils.GetExtensionForFileName(sampleRequest.sampleFileName);
                    string newFileName = Utils.GetFileNameForId(sampleRequest._id, sampleRequest.sampleFileName);

                    Utils.UploadFormFile(
                        form.Files[sampleRequest.sampleFileName],
                        Config.UploadStagingContainerName,
                        newFileName);

                    sampleRequest.sampleFileName = newFileName;
                    sampleRequest.fileExtension = ext;
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
                string newFileName = Utils.GetFileNameForId(
                    samplePackRequest._id,
                    samplePackRequest.imageFileName
                );

                Utils.UploadFormFile(
                    form.Files[transaction.request.imageFileName],
                    Config.UploadStagingContainerName,
                    newFileName);

                samplePackRequest.imageFileName = newFileName;
            }
            catch (Exception e)
            {
                //TODO: Rollback the upload(s)
                log.LogError($"Image upload failed {e.Message}", e);
                return new BadRequestResult();
            }

            //TODO: What if anything should we do w/ this orchestration Id?
            var orchestrationId = await starter.StartNewAsync(
                OrchestratorNames.UpsertSamplePack,
                transaction
            );

            return new OkObjectResult(orchestrationId);
        }

    }
}
