using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vp.models;
using vp.services;
using vp.util;

namespace vp.orchestrations.upsertSamplePack
{
    public class UpsertSamplePackActivities
    {
        private static ISamplePackService _samplePackService;

        public UpsertSamplePackActivities(ISamplePackService samplePackService)
        {
            _samplePackService = samplePackService;
        }

        [FunctionName(ActivityNames.UpsertSamplePackMetadata)]
        public async Task<SamplePack<Sample>> UpsertSamplePackMetadata(
            [ActivityTrigger] SamplePack<Sample> samplePack)
        {
            var result = await _samplePackService.AddSamplePack(samplePack);
            return result;
        }


        [FunctionName(ActivityNames.UpsertSamplePackTransferImage)]
        public async Task<UpsertSamplePackTransaction> UpsertSamplePackTransferImage(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
        )
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient destContainer = _blobServiceClient.GetBlobContainerClient(Config.CoverArtContainerName);
            var destBlob = destContainer.GetBlobClient(upsertSamplePackTransaction.request.imgUrl);
            
            await destBlob.StartCopyFromUriAsync(BlobFactory.GetBlobSasToken(Config.UploadStagingContainerName, upsertSamplePackTransaction.request.imgUrl));

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.CleanupStagingData)]
        public async Task<UpsertSamplePackTransaction> CleanupStagingData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSamplePackTransaction
)
        {
            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);

            BlobContainerClient srcContainer = _blobServiceClient.GetBlobContainerClient(Config.UploadStagingContainerName);
            var imgBlob = srcContainer.GetBlobClient(upsertSamplePackTransaction.request.imgUrl);

            await imgBlob.DeleteIfExistsAsync();

            foreach(var sampleRequest in upsertSamplePackTransaction.request.samples)
            {
                var sampleBlob = srcContainer.GetBlobClient(sampleRequest.clipUri);
                await sampleBlob.DeleteIfExistsAsync();
            }

            return upsertSamplePackTransaction;
        }

        [FunctionName(ActivityNames.UpsertStripeData)]

        public static async Task<UpsertSamplePackTransaction> UpsertStripeData(
            [ActivityTrigger] UpsertSamplePackTransaction upsertSammpleTransaction,
            ILogger log)
        {
            var sampleMetadata = upsertSammpleTransaction.request;
            var account = upsertSammpleTransaction.account;

            var sampleIds = JsonConvert.SerializeObject(
                sampleMetadata.samples.Select(sample => sample._id).ToArray());

            var sampleDescriptions = sampleMetadata.samples.Aggregate("", (acc, sample) =>
            {
                return $"{(acc == "" ? "" : $"{acc}, ")}{sample.name}";
            });

            var options = new Stripe.ProductCreateOptions
            {
                //TODO: Magic number
                Name = sampleMetadata.name,
                Description = $"{sampleMetadata.description}: {sampleDescriptions}",

                //Default to the currency of the User Account (Probably is affiliated w/ the account?)
                DefaultPriceData = new Stripe.ProductDefaultPriceDataOptions
                {
                    Currency = account.DefaultCurrency,
                    UnitAmountDecimal = sampleMetadata.cost
                },
                Metadata = new Dictionary<string, string>
                {
                    { "accountId", $"{account.Id}" },
                    { "sampleIds", $"{sampleIds}"}
                }
            };

            var service = new Stripe.ProductService();
            var stripeProduct = await service.CreateAsync(options);
            sampleMetadata.priceId = stripeProduct.DefaultPriceId;
            sampleMetadata.sellerId = account.Id;

            upsertSammpleTransaction.request = sampleMetadata;

            return upsertSammpleTransaction;
        }
    }
}
