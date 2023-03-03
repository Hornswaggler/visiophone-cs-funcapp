using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public class PurchaseService : BaseEntity<Purchase>, IPurchaseService
    {
        private readonly Container _purchasesContainer;

        public PurchaseService(CosmosClient cosmosClient) : base( cosmosClient) {
            _purchasesContainer = _cosmosClient.GetContainer(Config.DatabaseName, Config.PurchaseCollectionName);
        }

        public async Task<Purchase> AddPurchase(Purchase purchase)
        {
            purchase.id = Guid.NewGuid().ToString();
            return await InsertOneAsync(_purchasesContainer, purchase);
        }

        public async Task<List<Purchase>> GetPurchases(string accountId)
        {
            return await FindWhere(_purchasesContainer, "accountId", accountId);
        }
    }
}
