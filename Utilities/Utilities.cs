using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Stripe;
using System.Text;
using vp.models;
using vp.services;

namespace vp.utilities
{
    public  class Utilities
    {
        private readonly SamplePackService samplePackService;
        private readonly BlobContainerClient coverArtContainer;
        private readonly BlobContainerClient sampleContainer;
        private readonly BlobContainerClient transcodeContainer;
        private readonly StripeService stripeService;

        public Utilities() {
            CosmosClient client = new(
                connectionString: Config.CosmosConnectionString
            );

            samplePackService = new SamplePackService(client);
            stripeService = new StripeService(client);

            BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
            sampleContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleFilesContainerName);
            transcodeContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleTranscodeContainerName);
            coverArtContainer = _blobServiceClient.GetBlobContainerClient(Config.CoverArtContainerName);
        }

        private void UploadBlob(string blobName, string samplePath, BlobContainerClient container, string mimeType)
        {
            var blobClient = container.GetBlockBlobClient(blobName);

            using (FileStream fs = new FileStream(samplePath, FileMode.Open))
            {
                int offset = 0;
                int counter = 0;
                List<string> blockIds = new List<string>();

                var bytesRemaining = fs.Length;
                do
                {
                    var dataToRead = Math.Min(bytesRemaining, 1 * 1024 * 1024);
                    byte[] data = new byte[dataToRead];
                    var dataRead = fs.Read(data, offset, (int)dataToRead);
                    bytesRemaining -= dataRead;
                    if (dataRead > 0)
                    {
                        var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(counter.ToString("d6")));
                        blobClient.StageBlock(blockId, new MemoryStream(data));
                        blockIds.Add(blockId);
                        counter++;
                    }
                } while (bytesRemaining > 0);

                var headers = new BlobHttpHeaders()
                {
                    ContentType = mimeType
                };
                blobClient.CommitBlockList(blockIds, headers);
            }
        }

        private List<T> LoadEntitiesFromFile<T>(string path) {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<List<T>>(json);
            }
        }

        public async Task<int> InitializeEnvironmentData(bool duplicates = false) {
            var sampleDataPath = Environment.GetEnvironmentVariable("SAMPLE_DATA_PATH");
            var samplePacks = LoadEntitiesFromFile<SamplePack<Sample>>(
                $"{sampleDataPath}/samplePacks/samplePacks.json"
            );

            foreach (var samplePack in samplePacks)
            {

                Dictionary<string, string> idMap = new Dictionary<string, string>();

                if(duplicates)
                {
                    var samplePackId = Guid.NewGuid().ToString();
                    idMap.Add(samplePackId, samplePack.id);
                    samplePack.id = samplePackId;

                    foreach(var sample in samplePack.samples)
                    {
                        var id = Guid.NewGuid().ToString();
                        idMap.Add(id, sample.id);
                        sample.id = id;
                    }
                } else
                {
                    idMap.Add(samplePack.id, samplePack.id);
                    foreach(var sample in samplePack.samples)
                    {
                        idMap.Add(sample.id, sample.id);
                    }
                }

                //Insert the stripe record
                var description = samplePack.samples.Aggregate("", (acc, sample) =>
                {
                    return $"{(acc == "" ? "" : $"{acc}, ")}{sample.name}";
                });

                try
                {
                    var result = stripeService.GetPrice(samplePack.priceId);
                } catch (StripeException e)
                {
                    if(e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("Did not find price for samplePack");

                        var newProduct = await stripeService.UpsertProduct(
                            samplePack.sellerId,
                            idMap[samplePack.id],
                            samplePack.name,
                            description,
                            "usd",
                            (decimal)samplePack.cost
                        );

                        samplePack.priceId = newProduct.DefaultPriceId;
                    }
                }

                //Insert the database record
                try
                {
                    await samplePackService.AddSamplePack(samplePack);
                } catch
                {
                    //consume (already exists)
                }

                //Move the Sample(s) to storage
                foreach (var sample in samplePack.samples)
                {
                    try
                    {
                        var blobName = $"{sample.id}.wav";
                        var sampleFilePath = $"{sampleDataPath}/samplePacks/{idMap[sample.id]}.wav";
                        UploadBlob(blobName, sampleFilePath, sampleContainer, "audio/wav");
                    } catch
                    {
                        //consume (already exists)
                    }

                }

                //Insert the transcode
                foreach (var sample in samplePack.samples)
                {
                    try
                    {
                        var blobName = $"{sample.id}.ogg";
                        var sampleFilePath = $"{sampleDataPath}/samplePacks/{idMap[sample.id]}.ogg";
                        UploadBlob(blobName, sampleFilePath, transcodeContainer, "audio/ogg");
                    } catch
                    {
                        //consume (already exists)
                    }

                }

                //Migrate cover art
                try
                {
                    var blobName = $"{samplePack.id}.png";
                    var sampleFilePath = $"{sampleDataPath}/samplePacks/{idMap[samplePack.id]}.png";
                    UploadBlob(blobName, sampleFilePath, coverArtContainer, "image/x-png");
                } catch
                {
                    //consume (already exists)
                }
            }

            return 0;
        }
    }
}
