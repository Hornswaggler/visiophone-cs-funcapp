using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using vp.models;
using vp.services;

namespace vp.orchestrations.upsertsample
{
    public class UpsertSampleActivities
    {
        private ISampleService _sampleService;

        public UpsertSampleActivities(
            ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [FunctionName(ActivityNames.UpsertStripeData)]

        public static async Task<UpsertSampleTransaction> UpsertStripeData(
            [ActivityTrigger] UpsertSampleTransaction upsertSammpleTransaction,
            ILogger log)
        {
            var sampleMetadata = upsertSammpleTransaction.request;
            var account = upsertSammpleTransaction.account;

            var options = new Stripe.ProductCreateOptions
            {
                //TODO: Magic number
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

            upsertSammpleTransaction.request = sampleMetadata;

            return upsertSammpleTransaction;
        }

        [FunctionName(ActivityNames.UpsertSample)]
        public async Task<Sample> UpsertSampleMetaData (
            [ActivityTrigger] Sample sample,
            ILogger log)
        {
            var result = await _sampleService.AddSample(sample);
            return result;
        }
    }
}
