using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using vp.models;
using vp.orchestrations.upsertSamplePack;
using vp.services;

namespace vp.orchestrations.rollbackSamplePackUploadOrchestrator
{
    public class RollbackSamplePackUploadActivities
    {
        private static IStorageService _storageService;
        private static ISamplePackService _samplePackService;

        public RollbackSamplePackUploadActivities(IStorageService storageService, ISamplePackService samplePackService) {
            _storageService = storageService;
            _samplePackService = samplePackService;
        }

        [FunctionName(ActivityNames.RollbackSamplePackUpload)]
        public async Task<UpsertSamplePackTransaction> RollbackSamplePackUpload(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Rolling back sample pack upload for transaction: {upsertSamplePackTransaction.request.id}");
                await _storageService.RollbackSampleTransactionBlobsForSamplePackTransaction(upsertSamplePackTransaction);
            }
            catch (Exception e)
            {
                var error = $"Failed to rollback sample pack upload for request: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.RollbackStripeProduct)]
        public async Task<UpsertSamplePackTransaction> RollbackStripeProduct(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Rolling back Stripe Product metadata for transaction: {upsertSamplePackTransaction.request.id}");
                var service = new Stripe.ProductService();
                await service.DeleteAsync(upsertSamplePackTransaction.request.productId);
            }
            catch (Exception e)
            {
                var error = $"Failed to rollback stripe product data for request: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.RollbackSamplePackMetadata)]
        public async Task<UpsertSamplePackTransaction> RollbackSamplePackMetadata(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Rolling back Sample pack metadata for transaction: {upsertSamplePackTransaction.request.id}");
                
                var result = await _samplePackService.GetSamplePackById(upsertSamplePackTransaction.request.id);
                if (result != null)
                {
                    await _samplePackService.DeleteSamplePack((SamplePack<Sample>)upsertSamplePackTransaction.request);
                }
            } catch(Exception e)
            {
                var error = $"Failed to rollback sample pack metadata for request: {upsertSamplePackTransaction.request.id}.";
                log.LogError(error, e);
                throw new Exception(error, e);
            }
            

            return upsertSamplePackTransaction;
        }
    }
}
