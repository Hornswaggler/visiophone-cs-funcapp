using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace vp
{
    public class SampleService : ISampleService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<SampleModel> _samples;

        public SampleService(MongoClient mongoClient, IConfiguration configuration)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _samples = _database.GetCollection<SampleModel>("samples");
        }

        public async Task AddSample(SampleModel sample)
        {
            await _samples.InsertOneAsync(sample);
        }

        public Task<List<SampleModel>> GetSamples() {
            return Task.FromResult(_samples.Find(s => true).ToList());
        }

    }
}

