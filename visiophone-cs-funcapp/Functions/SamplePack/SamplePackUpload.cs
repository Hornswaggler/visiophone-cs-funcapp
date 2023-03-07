
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using vp.functions.stripe;
using vp.orchestrations;
using vp.orchestrations.upsertSamplePack;
using vp.services;
using vp.util;

namespace vp.functions.samplepack {
    public class SamplePackUpload : AuthStripeBase
    {
        public SamplePackUpload(IUserService userService, IStripeService stripeService, IValidationService validationService) 
            : base(userService, stripeService, validationService) { }

        [FunctionName(FunctionNames.SamplePackUpload)]
        public async Task<IActionResult> Run (
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            StripeProfileResult account;
            try
            {
                account = await AuthorizeStripeUser(req);
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

                var errors = await _validationService.ValidateEntity(samplePackRequest, "samplePack");
                if(errors.Count > 0)
                {
                    var errorstring = JsonConvert.SerializeObject(errors.Keys);
                    log.LogError($"SamplePackUpload failed validation: {errorstring}");
                    return new BadRequestObjectResult(errorstring);
                }

                samplePackRequest.id = Guid.NewGuid().ToString();
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
                var sampleRequests = transaction.request.samples;
                foreach (var sampleRequest in sampleRequests)
                {
                    sampleRequest.id = Guid.NewGuid().ToString();

                    log.LogInformation($"samplePack: {transaction.request.id}, sample: {sampleRequest.id}");

                    var ext = Utils.GetExtensionForFileName(sampleRequest.clipUri);
                    string newFileName = Utils.GetFileNameForId(sampleRequest.id, sampleRequest.clipUri);

                    log.LogInformation($"Sample file name: {newFileName}");

                    Utils.UploadFormFile(
                        form.Files[sampleRequest.clipUri],
                        Config.UploadStagingContainerName,
                        newFileName);

                    sampleRequest.clipUri = newFileName;
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
                    samplePackRequest.id,
                    samplePackRequest.imgUrl
                );

                Utils.UploadFormFile(
                    form.Files[transaction.request.imgUrl],
                    Config.UploadStagingContainerName,
                    newFileName);

                samplePackRequest.imgUrl = newFileName;
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
