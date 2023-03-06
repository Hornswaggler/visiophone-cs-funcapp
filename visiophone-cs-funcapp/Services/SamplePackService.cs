using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.functions.samplepack;
using vp.models;

namespace vp.services
{
    public class SamplePackService : BaseEntity<SamplePack<Sample>>, ISamplePackService
    {
        private readonly Container _samplePackContainer;

        public SamplePackService(CosmosClient cosmosClient) : base(cosmosClient) {
            _samplePackContainer = _cosmosClient.GetContainer(Config.DatabaseName, Config.SamplePackCollectionName);
        }

        protected async Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksByField(SearchQueryRequest request, string field)
        {
            return await FindWhereRegexMatch(_samplePackContainer, request.query, field, request.index);
        }

        public async Task<SamplePack<Sample>> GetSamplePackById(string samplePackId)
        {
            return await GetById(_samplePackContainer, samplePackId);
        }


        public async Task<SamplePack<Sample>> AddSamplePack(SamplePack<Sample> samplePack) {
            samplePack.id = samplePack.id;
            await _samplePackContainer.CreateItemAsync(samplePack);

            return samplePack;
        }

        public async Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksByName(SearchQueryRequest request)
        {
            return await GetSamplePacksByField(request, "name");
        }

        public async Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksBySellerId(SearchQueryRequest request)
        {
            return await GetSamplePacksByField(request, "sellerId");
        }

        public async Task<List<SamplePack<Sample>>> GetSamplePackPurchasesByPriceIds(List<string> priceIds)
        {

            return await FindWhereIn(_samplePackContainer, "priceId", priceIds);
        }
    }
}
