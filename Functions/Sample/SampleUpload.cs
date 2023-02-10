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
using vp.orchestrations.upsertsample;
using vp.services;

namespace vp.functions.sample
{
    public class SampleUpload : AuthBase
    {
        public SampleUpload(IUserService userService, IValidationService validationService) : base(userService, validationService) { }

        [FunctionName(FunctionNames.SampleUpload)]
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

            UpsertSampleRequest upsertSampleRequest = null;
            try
            {
                var formData = req.Form["data"];
                upsertSampleRequest = JsonConvert.DeserializeObject<UpsertSampleRequest>(formData);

                var errors = await _validationService.ValidateEntity(upsertSampleRequest, "sample");
                if (errors.Count > 0)
                {
                    var errorstring = JsonConvert.SerializeObject(errors.Keys);
                    log.LogError($"Sample failed validation: {errorstring}");
                    return new BadRequestObjectResult(errorstring);
                }

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
                    form.Files[upsertSampleRequest.clipUri], 
                    Config.SampleBlobContainerName, 
                    upsertSampleRequest.clipUri);
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

            return new OkObjectResult(orchestrationId);
        }
    }
}
