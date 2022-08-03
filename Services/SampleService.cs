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

