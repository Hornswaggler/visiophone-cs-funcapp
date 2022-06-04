using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using vp.Models;

namespace vp.Services
{
    public class SampleService : ISampleService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Sample> _samples;

        public SampleService(MongoClient mongoClient, IConfiguration configuration)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _samples = _database.GetCollection<Sample>("samples");
        }

        public async Task AddSample(Sample sample)
        {
            await _samples.InsertOneAsync(sample);
        }

    }
}

