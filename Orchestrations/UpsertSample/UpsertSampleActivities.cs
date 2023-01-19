using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.services;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleActivities
    {
        private readonly ISampleService _sampleService;

        public UpsertSampleActivities (
            ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [FunctionName(ActivityNames.UpsertStripeData)]

        public static async Task<UpsertSampleTransaction> UpsertStripeData(
            [ActivityTrigger] UpsertSampleTransaction upsertSammpleTransaction,
            ILogger log)
        {
            var sampleMetadata = upsertSammpleTransaction.request.sampleMetadata;
            var account = upsertSammpleTransaction.account;

            var options = new Stripe.ProductCreateOptions
            {
                Name = sampleMetadata.name,
                Description = sampleMetadata.description,
                DefaultPriceData = new Stripe.ProductDefaultPriceDataOptions
                {
                    Currency = "USD",
                    UnitAmountDecimal = sampleMetadata.cost
                },
                Metadata = new Dictionary<string, string>
                {
                    { "accountId", $"{account.Id}" },
                }
            };

            var service = new Stripe.ProductService();
            var stripeProduct = await service.CreateAsync(options);
            sampleMetadata.priceId = stripeProduct.DefaultPriceId;
            sampleMetadata.sellerId = account.Id;

            upsertSammpleTransaction.request.sampleMetadata = sampleMetadata;

            return upsertSammpleTransaction;
        }

        [FunctionName(ActivityNames.UpsertSampleMetaData)]

        public async Task<UpsertSampleTransaction> UpsertSampleMetaData (
            [ActivityTrigger] UpsertSampleTransaction upsertSampleDTO,
            ILogger log)
        {
            var sampleMetadata = upsertSampleDTO.request.sampleMetadata;

            upsertSampleDTO.request.sampleMetadata = await _sampleService.AddSample(sampleMetadata);
            return upsertSampleDTO;
        }
    }
}
