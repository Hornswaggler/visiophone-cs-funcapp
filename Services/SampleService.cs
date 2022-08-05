using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vp.models;
using vp.services;

namespace vp.services
{
    public class SampleService : ISampleService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<SampleRequest> _samples;
        private readonly int _itemsPerPage = 50;

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


            _samples = _database.GetCollection<SampleRequest>("samples");
        }
 
        public async Task AddSample(SampleRequest sample)
        {
            await _samples.InsertOneAsync(sample);
        }

        public async Task<List<SampleRequest>> GetSamples(SampleRequest request) {
            var builder = Builders<SampleRequest>.Filter;
            BsonRegularExpression queryExpr = new BsonRegularExpression(new Regex($"^{request.description}.*", RegexOptions.IgnoreCase));
            FilterDefinition<SampleRequest> filter = builder.Regex("description", queryExpr);
            var results = await _samples.FindAsync<SampleRequest>(filter);

            return results.ToList();

            //return Task.FromResult<SampleRequest>();
                //
                //.Limit(_itemsPerPage).ToList());
        }

    }
}

