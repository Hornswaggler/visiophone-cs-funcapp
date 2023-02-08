using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public class SampleService : MongoSearchBase<Sample>, ISampleService
    {

        private readonly IMongoCollection<Sample> _samples;

        public SampleService(MongoClient mongoClient) : base(mongoClient)
        {
            _samples = _database.GetCollection<Sample>(Config.SampleCollectionName);
        }

        protected async Task<SearchQueryResult<Sample>> GetSamplesByField(SearchQuery request, string field)
        {
            return await FindByField(_samples, request.query, field, request.index);
        }

        public async Task<Sample> AddSample(Sample sample)
        {
            await _samples.InsertOneAsync(sample);
            return sample;
        }

        public async Task<List<Sample>> GetSamples(List<string> priceIds)
        {
            var filter = Builders<Sample>.Filter.In(p => p.priceId, priceIds);
            var query = await _samples.FindAsync(filter);
            var result = await query.ToListAsync();
            return result;
        }

        public async Task<SearchQueryResult<Sample>> GetSamplesByName(SearchQuery request)
        {
            return await GetSamplesByField(request, "name");
        }


    }
}

