using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
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
        private readonly int _itemsPerPage = 10;

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

        public Task<List<SampleRequest>> GetSamples(SampleRequest request) {
            return Task.FromResult(_samples.Find(s => true)
                .Skip(_itemsPerPage * request.page)
                .Limit(_itemsPerPage).ToList());
        }

    }
}

