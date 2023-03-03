using MongoDB.Driver;
using System.Threading.Tasks;
using vp.models;
using vp.functions.samplepack;
using System;

namespace vp.services
{
    public class SampleService : MongoSearchBase<Sample>, ISampleService
    {

        private readonly IMongoCollection<Sample> _samples;

        public SampleService(MongoClient mongoClient) : base(mongoClient)
        {
            _samples = _database.GetCollection<Sample>(Config.SampleCollectionName);
        }

        protected async Task<SearchQueryResult<Sample>> GetSamplesByField(SearchQueryRequest request, string field)
        {
            return await FindByField(_samples, request.query, field, request.index);
        }

        public async Task<Sample> AddSample(Sample sample)
        {
            try
            {
                await _samples.InsertOneAsync(sample);

            } catch(Exception e)
            {
                int i = 0;
                i++;
            }
            return sample;
        }


        public async Task<SearchQueryResult<Sample>> GetSamplesByName(SearchQueryRequest request)
        {
            return await GetSamplesByField(request, "name");
        }


    }
}

