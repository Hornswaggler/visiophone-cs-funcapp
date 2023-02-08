using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public class PurchaseService : MongoSearchBase<Purchase>, IPurchaseService
    {
        private readonly IMongoCollection<Purchase> _purchases;

        public PurchaseService(MongoClient mongoClient) : base(mongoClient) {
            _purchases = _database.GetCollection<Purchase>(Config.PurchaseCollectionName);
        }

        public async Task<Purchase> AddPurchase(Purchase purchase)
        {
            return await InsertOne(_purchases, purchase);
        }

        public async Task<List<Purchase>> GetPurchases(string accountId)
        {
            var purchaseQuery = await _purchases.FindAsync<Purchase>(p => p.accountId.Equals(accountId));
            return await purchaseQuery.ToListAsync();
        }
    }
}
