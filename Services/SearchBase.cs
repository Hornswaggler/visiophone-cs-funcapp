using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vp.DTO;

namespace vp.services
{
    public abstract class MongoSearchBase<T>
    {
        protected readonly IMongoClient _mongoClient;
        protected readonly IMongoDatabase _database;

        public MongoSearchBase(MongoClient mongoClient) {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase(Config.DatabaseName);
        }

        protected async Task<SearchQueryResult<T>> FindByField(IMongoCollection<T> collection, string query = "", string field = "", int index = 0) {
            var builder = Builders<T>.Filter;
            BsonRegularExpression queryExpr = new BsonRegularExpression(new Regex($"^{query}.*", RegexOptions.IgnoreCase));
            FilterDefinition<T> filter = builder.Regex(field, queryExpr);

            FindOptions<T> options = new FindOptions<T>
            {
                Limit = Config.ResultsPerRequest,
                Skip = index
            };

            var result = (await collection.FindAsync<T>(filter, options)).ToList();
            return new SearchQueryResult<T>
            {
                data = result,
                nextResultIndex = result.Count == 0 ? -1 : index + Config.ResultsPerRequest
            };
        }

        protected async Task<T> InsertOne(IMongoCollection<T> collection, T entity)
        {
            await collection.InsertOneAsync(entity);
            return entity;
        }

    }
}
