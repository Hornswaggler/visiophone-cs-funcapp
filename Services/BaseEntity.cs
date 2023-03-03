using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vp.functions.samplepack;

namespace vp.services
{
    public abstract class BaseEntity<T>
    {
        protected readonly CosmosClient _cosmosClient;

        public BaseEntity(CosmosClient cosmosClient) {
            _cosmosClient = cosmosClient;
        }

        protected async Task<SearchQueryResult<T>> FindWhereRegexMatch(Container container, string query = "", string field = "", int index = 0, bool asc = true) {
            var regexMatch = $"^{query}.*";
            var statement = $"SELECT * FROM {container.Id} f";

            if(query != "")
            {
                statement = $"{statement} WHERE RegexMatch (f.{field}, \"{regexMatch}\",\"i\")";
            }

            statement = $"{statement} OFFSET {index} LIMIT {Config.ResultsPerRequest}";

            var queryDefinition = new QueryDefinition(
               query: statement
            );

            var items = await container.GetItemQueryIterator<T>(queryDefinition: queryDefinition).ReadNextAsync();
  
            var result =  new SearchQueryResult<T>
            {
                data = items.ToList(),
                nextResultIndex = items.Count == 0
                    ? -1
                    : index + Config.ResultsPerRequest
            };

            return result;
        }

        protected async Task<T> GetByField(Container container, string field, string value)
        {
            var items = await container.GetItemQueryIterator<T>(
                queryDefinition: new QueryDefinition(
                    query: $"SELECT * FROM {container.Id} f WHERE f.{field} = \"{value}\""
                )
            )
            .ReadNextAsync();

            return items.FirstOrDefault();
        }

        protected async Task<List<T>> FindWhere(Container container, string field, string value)
        {
            var items = await container.GetItemQueryIterator<T>(
                queryDefinition: new QueryDefinition(
                    query: $"SELECT * FROM {container.Id} f WHERE f.{field} = \"{value}\""
                )
            )
            .ReadNextAsync();

            return items.ToList();
        }

        protected async Task<List<T>> FindWhereIn(Container container, string field, List<string> values) {
            if(values.Count == 0)
            {
                return new List<T>();
            }

            var newValues = values.Select(value => $"\"{value}\"");
            var inClause = string.Join(",", newValues);
            var theQuery = $"SELECT * FROM {container.Id} f WHERE f.{field} IN ({inClause})";

            var items = await container.GetItemQueryIterator<T>(
                queryDefinition: new QueryDefinition(
                    query: theQuery
                )
            )
            .ReadNextAsync();

            return items.ToList();
        }

        protected async Task<T> GetById(Container container, string id)
        {
            var items = await container.GetItemQueryIterator<T>(
                queryDefinition: new QueryDefinition(
                    query: $"SELECT * FROM {container.Id} f WHERE f.id = \"{id}\""
                )
            )
            .ReadNextAsync();

            return items.FirstOrDefault();
        }

        protected async Task<T> InsertOneAsync(Container container, T entity)
        {
            await container.CreateItemAsync(entity);
            return entity;
        }
    }
}
