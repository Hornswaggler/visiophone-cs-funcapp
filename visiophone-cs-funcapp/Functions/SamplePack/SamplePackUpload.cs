﻿
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
        public async Task<IActionResult> Run(
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
            IFormCollection form = req.Form;

            //Validate Request
            try
            {
                var formData = form["data"];
                samplePackRequest = JsonConvert.DeserializeObject<UpsertSamplePackRequest>(formData);

                var errors = await _validationService.ValidateEntity(samplePackRequest, "samplePack");
                if (errors.Count > 0)
                {
                    var errorstring = JsonConvert.SerializeObject(errors.Keys);
                    log.LogError($"SamplePackUpload failed validation: {errorstring}");
                    return new BadRequestObjectResult(errorstring);
                }
            }
            catch (Exception e)
            {
                log.LogError($"Samplepack validation failed: {e.Message}", e);
                return new BadRequestResult();
            }

            //Update Samplepack Metadata
            try
            {
                samplePackRequest.id = Guid.NewGuid().ToString();
                //TODO: Remove this... should be handled in the front end
                samplePackRequest.cost = samplePackRequest.cost * 100;
                samplePackRequest.stagingImgBlobPath = Utils.GetFileNameForId(samplePackRequest.id, samplePackRequest.imgUrl);
                transaction = new UpsertSamplePackTransaction
                {
                    account = account,
                    userName = userName,
                    request = samplePackRequest
                };

                foreach (var sample in samplePackRequest.samples)
                {
                    sample.id = Guid.NewGuid().ToString();
                    sample.fileExtension = Utils.GetExtensionForFileName(sample.clipUri);
                    sample.blobName = Utils.GetFileNameForId(sample.id, sample.clipUri);
                }
            } catch (Exception e)
            {
                log.LogError($"Failed to generate samplepack metadata {e.Message}", e);
                return new BadRequestResult();
            }

            //Upload the sample files to the staging area
            try
            {
                UploadSamplePackFormFiles(transaction, form);
                //TODO: Delete this...
                //throw new Exception("The shit hit the fan! Everything is sideways");
            }
            catch (Exception e)
            {
                log.LogError($"Samplepack sample uploads failed {e.Message}", e);
                RollbackSamplePackUpload(starter, transaction, log);
                return new BadRequestResult();
            }

            //TODO: What if anything should we do w/ this orchestration Id?
            var orchestrationId = await starter.StartNewAsync(
                OrchestratorNames.UpsertSamplePack,
                transaction
            );

            return new OkObjectResult(orchestrationId);
        }

        private void RollbackSamplePackUpload(
            IDurableOrchestrationClient starter,
            UpsertSamplePackTransaction transaction,
            ILogger log)
        {
            try
            {
                 starter.StartNewAsync(
                     OrchestratorNames.RollbackSamplePackUpsert,
                     transaction
                 );
            }
            catch (Exception ie)
            {
                log.LogError($"Failed to post samplepack rollback, {ie.Message}", ie);
            }
        }

        private void UploadSamplePackFormFiles(
            UpsertSamplePackTransaction transaction,
            IFormCollection form)
        {
            foreach (var sample in transaction.request.samples)
            {
                Utils.UploadFormFile(
                    form.Files[sample.clipUri],
                    Config.UploadStagingContainerName,
                    sample.blobName,
                    $"{transaction.request.id}/{Config.BlobImportDirectoryName}/");
            }

            Utils.UploadFormFile(
                form.Files[transaction.request.imgUrl],
                Config.UploadStagingContainerName,
                transaction.request.stagingImgBlobPath,
                $"{transaction.request.id}/{Config.BlobImportDirectoryName}/");
            }

    }
}
