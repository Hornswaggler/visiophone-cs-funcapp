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
        private readonly IMongoCollection<Purchase> _purchases;

        // TODO This should be configurable
        public static int ITEMS_PER_PAGE = 50;

        public SampleService(MongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _samples = _database.GetCollection<Sample>("samples");
            _purchases = _database.GetCollection<Purchase>("purchases");
        }

        public async Task<Sample> AddSample(Sample sample)
        {
            await _samples.InsertOneAsync(sample);
            return sample;
        }

        public async Task<Purchase> AddPurchase(Purchase purchase) {
            await _purchases.InsertOneAsync(purchase);
            return purchase;
        }
        
        public async Task<List<Purchase>> GetPurchases(string accountId) {
            var purchaseQuery = await _purchases.FindAsync<Purchase>(p => p.accountId.Equals(accountId));
            return await purchaseQuery.ToListAsync();
        }

        public async Task<List<Sample>> GetSamples(List<string> priceIds)
        {
            var filter = Builders<Sample>.Filter.In(p => p.priceId, priceIds);
            var query = await _samples.FindAsync(filter);
            var result = await query.ToListAsync();
            return result;
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

