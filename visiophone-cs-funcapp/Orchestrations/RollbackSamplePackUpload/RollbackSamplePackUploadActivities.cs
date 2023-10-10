using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vp.orchestrations.upsertSamplePack;
using vp.services;
using vp.util;

namespace vp.orchestrations.rollbackSamplePackUploadOrchestrator
{
    public class RollbackSamplePackUploadActivities
    {
        private static IStorageService _storageService;

        public RollbackSamplePackUploadActivities(IStorageService storageService) {
            _storageService = storageService;
        }

        [FunctionName(ActivityNames.RollbackSamplePackUpload)]
        public async Task<UpsertSamplePackTransaction> RollbackSamplePackUpload(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                //TODO: Test this...
                await _storageService.DeleteUploadsForSamplePackTransaction(upsertSamplePackTransaction);

                //TODO: DELETE DATA FROM Transcodes / cover-art storage here...



                return upsertSamplePackTransaction;
            }
            catch (Exception e)
            {
                var error = $"Failed to rollback sample pack upload for request: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }
        }

        [FunctionName(ActivityNames.RollbackStripeProduct)]
        public async Task<UpsertSamplePackTransaction> RollbackStripeProduct(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                var service = new Stripe.ProductService();
                await service.DeleteAsync(upsertSamplePackTransaction.request.productId);

                return upsertSamplePackTransaction;
            }
            catch (Exception e)
            {
                var error = $"Failed to rollback stripe product data for request: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

        }

        [FunctionName(ActivityNames.RollbackSamplePackMetadata)]
        public async Task<UpsertSamplePackTransaction> RollbackSamplePackMetadata(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            //TODO: Check for samplePack id here, if it exists... delete the record
            // if it does not exist, it was never created in the database to begin with...



            return null;
        }
    }
}
