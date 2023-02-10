using MongoDB.Driver;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public class SamplePackService : MongoSearchBase<SamplePack<Sample>>, ISamplePackService
    {
        private readonly IMongoCollection<SamplePack<Sample>> _samplePacks;

        public SamplePackService(MongoClient mongoClient) : base(mongoClient) {
            _samplePacks = _database.GetCollection<SamplePack<Sample>>(Config.SamplePackCollectionName);
        }

        protected async Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksByField(SearchQuery request, string field)
        {
            return await FindByField(_samplePacks, request.query, field, request.index);
        }

        public async Task<SamplePack<Sample>> AddSamplePack(SamplePack<Sample> samplePack) {
            await _samplePacks.InsertOneAsync(samplePack);
            return samplePack;
        }

        public async Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksByName(SearchQuery request)
        {
            return await GetSamplePacksByField(request, "name");
        }

    }
}
