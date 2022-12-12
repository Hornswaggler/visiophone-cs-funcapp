using MongoDB.Driver;
using System.Threading.Tasks;
using vp.models;

namespace vp.Services
{
    public class CheckoutSessionService : ICheckoutSessionService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<CheckoutSession> _checkoutSession;

        public CheckoutSessionService(MongoClient mongoClient) {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _checkoutSession = _database.GetCollection<CheckoutSession>("checkoutSession");
        }
        public async Task<CheckoutSession> CreateCheckoutSession(CheckoutSession checkoutSession) {
            await _checkoutSession.InsertOneAsync(checkoutSession);
            return checkoutSession;
        }
    }
}
