using MongoDB.Driver;
using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public class SamplePackService : ISamplePackService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<SamplePack> _samplePacks;

        public SamplePackService(MongoClient mongoClient) {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _samplePacks = _database.GetCollection<SamplePack>("samplePacks");
        }

        public async Task<SamplePack> AddSamplePack(SamplePack samplePack) {
            await _samplePacks.InsertOneAsync(samplePack);
            return samplePack;
        }
    }
}
