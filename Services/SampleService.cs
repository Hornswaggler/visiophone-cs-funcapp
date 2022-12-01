using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;
using System.Linq;

namespace vp.services
{
    public class SampleService : ISampleService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Sample> _samples;

        // TODO This should be configurable
        public static int ITEMS_PER_PAGE = 50;

        public SampleService(MongoClient mongoClient, IConfiguration configuration)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");

            // TODO Make this into a provisioning script to be run in the deployment pipeline....
            // This will provision the Sample collection (specifically for local dev)
            //var bson = new BsonDocument
            //{
            //    { "customAction", "CreateCollection" },
            //    { "collection", "samples" },//update CollectionName
            //    { "shardKey", "key" }, //update ShardKey
            //    { "offerThroughput", 400} //update Throughput
            //};
            //var shellCommand = new BsonDocumentCommand<BsonDocument>(bson);
            //_database.RunCommand(shellCommand);

            _samples = _database.GetCollection<Sample>("samples");
        }
 
        public async Task<Sample> AddSample(Sample sample)
        {
            await _samples.InsertOneAsync(sample);
            return sample;
        }

        public async Task<SampleQueryResult> GetSamples(SampleRequest request) {
            var builder = Builders<Sample>.Filter;
            BsonRegularExpression queryExpr = new BsonRegularExpression(new Regex($"^{request.query}.*", RegexOptions.IgnoreCase));
            FilterDefinition<Sample> filter = builder.Regex("description", queryExpr);

            FindOptions<Sample> options = new FindOptions<Sample>
            {
                Limit = ITEMS_PER_PAGE,
                Skip = request.index
            };

            var samples = await _samples.FindAsync<Sample>(filter, options);
            List<Sample> result = samples.ToList();

            return new SampleQueryResult
            {
                samples = result,
                nextResultIndex = result.Count == 0 ? -1 : request.index + ITEMS_PER_PAGE
            };
        }

        public async Task<List<Sample>> GetSamplesById(IEnumerable<string> sampleIds)
        {
            var samples = await _samples.FindAsync<Sample>(sample => sampleIds.Contains(sample._id));
            return samples.ToList<Sample>();
        }

        public async Task<Sample> GetSampleById(string sampleId)
        {
            var sampleQuery = await _samples.FindAsync<Sample>(sample => sample._id.Equals(sampleId));
            return await sampleQuery.FirstOrDefaultAsync<Sample>();
        }
    }
}

