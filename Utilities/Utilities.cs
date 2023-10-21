using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System.Text;
using vp.models;
using vp.services;

namespace vp.utilities
{
    public  class Utilities
    {
        CosmosClient cosmosClient;
        BlobServiceClient _blobServiceClient;
        ILogger log;

        public Utilities(ILogger log) {

            this.log = log;

            //TODO: Region hardcoded...
            cosmosClient = new (
                connectionString: Config.CosmosConnectionString,
                new CosmosClientOptions()
                {
                    ApplicationRegion = Regions.EastUS2,
                }
            );

            _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
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
            BlobContainerClient coverArtContainer = _blobServiceClient.GetBlobContainerClient(Config.SamplePackCoverArtBlobContainerName);
            BlobContainerClient avatarContainer = _blobServiceClient.GetBlobContainerClient(Config.AvatarBlobContainerName);
            BlobContainerClient sampleContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleHDBlobContainerName);
            BlobContainerClient transcodeContainer = _blobServiceClient.GetBlobContainerClient(Config.SamplePreviewBlobContainerName);

            StripeService stripeService = new StripeService(cosmosClient);
            SamplePackService samplePackService = new SamplePackService(cosmosClient);

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

                //Migrate avatars
                var testAccountIds = new List<string> { "a2492c46-47cc-4b51-9cf1-65e1dfafe68e", "a2c87319-bd83-4b88-8f27-80f91d1d8d80" };
                foreach(var accountId in testAccountIds)
                {
                    try
                    {
                        var blobName = $"{accountId}.png";
                        var sampleFilePath = $"{sampleDataPath}/avatars/{accountId}.png";
                        UploadBlob(blobName, sampleFilePath, avatarContainer, "image/x-png");
                    }
                    catch
                    {
                        //consume (already exists)
                    }
                }
               
            }

            return 0;
        }

        public async Task InitializeDatabaseSchema() {
            try
            {
                Database db = await cosmosClient.CreateDatabaseIfNotExistsAsync(Config.DatabaseName);

                await db.CreateContainerIfNotExistsAsync(
                    new ContainerProperties
                    {
                        Id = Config.SamplePackCollectionName,
                        PartitionKeyPath = Config.SamplePackCollectionPartitionKey
                    }
                );

                await db.CreateContainerIfNotExistsAsync(
                    new ContainerProperties
                    {
                        Id = Config.StripeProfileCollectionName,
                        PartitionKeyPath = "/accountId"
                    }
                );

                await db.CreateContainerIfNotExistsAsync(
                    new ContainerProperties
                    {
                        Id = Config.PurchaseCollectionName,
                        PartitionKeyPath = "/accountId"
                    }
                );
            } catch (Exception e)
            {
                log.LogError(e.Message, e);
            }
        }

        public async Task InitializeStorage() {
            try
            {
                await _blobServiceClient.CreateBlobContainerAsync(
                    Config.AvatarBlobContainerName,
                    PublicAccessType.Blob
                );

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to create: {Config.AvatarBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.CreateBlobContainerAsync(Config.SamplePreviewBlobContainerName, PublicAccessType.Blob);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to create: {Config.SamplePreviewBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.CreateBlobContainerAsync(Config.UploadStagingBlobContainerName);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to create: {Config.UploadStagingBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.CreateBlobContainerAsync(
                    Config.SamplePackCoverArtBlobContainerName, PublicAccessType.Blob
                );

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.SamplePackCoverArtBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.CreateBlobContainerAsync(Config.SampleHDBlobContainerName, PublicAccessType.Blob);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.SampleHDBlobContainerName}: {e.Message}", e);
            }
        }

        public async Task DeleteDatabase() {
            try
            {
                var samplePackClient = cosmosClient.GetDatabase(Config.DatabaseName);
                await samplePackClient.DeleteAsync();
            } 
            catch (Exception e)
            {
                log.LogWarning($"failed to delete database: {Config.DatabaseName}", e);
            }
        }

        public async Task DeleteStorage() {
            try
            {
                await _blobServiceClient.DeleteBlobContainerAsync(Config.UploadStagingBlobContainerName);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.UploadStagingBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.DeleteBlobContainerAsync(Config.AvatarBlobContainerName);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.AvatarBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.DeleteBlobContainerAsync(Config.SamplePreviewBlobContainerName);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.SamplePreviewBlobContainerName}: {e.Message}", e);
            }

            //try
            //{
            //    await _blobServiceClient.DeleteBlobContainerAsync(Config.UploadStagingBlobContainerName);

            //}
            //catch (Exception e)
            //{
            //    log.LogWarning($"failed to delete: {Config.UploadStagingBlobContainerName}: {e.Message}", e);
            //}

            try
            {
                await _blobServiceClient.DeleteBlobContainerAsync(Config.SamplePackCoverArtBlobContainerName);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.SamplePackCoverArtBlobContainerName}: {e.Message}", e);
            }

            try
            {
                await _blobServiceClient.DeleteBlobContainerAsync(Config.SampleHDBlobContainerName);

            }
            catch (Exception e)
            {
                log.LogWarning($"failed to delete: {Config.SampleHDBlobContainerName}: {e.Message}", e);
            }
        }
    }
}
