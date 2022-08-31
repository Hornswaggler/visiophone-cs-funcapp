using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using vp.DTO;
using vp.Models;
using vp.services;

namespace vp.services
{
    public class UserService : IUserService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<UserProfileModel> _users;

        public UserService(MongoClient mongoClient, IConfiguration configuration) {
            //TODO: move this up a layer...
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _users = _database.GetCollection<UserProfileModel>("users");
        }

        public async Task<UserProfileModel> GetUserProfile(UserProfileRequest request)
        {
            var result = await _users.FindAsync(u => u._id.Equals(request.userId));

            int i = 0;
            i++;

            return await result.FirstOrDefaultAsync();

            //var builder = Builders<UserProfileModel>.Filter;
            //BsonRegularExpression queryExpr = new BsonRegularExpression(new Regex($"^{request.query}.*", RegexOptions.IgnoreCase));

        }

        public async Task<UserProfileModel> SetUserProfile(UserProfileModel userProfile)
        {
            await _users.InsertOneAsync(userProfile);

            int i = 0;
            i++;

            return userProfile;

            //return await result.FirstOrDefaultAsync();

            //var builder = Builders<UserProfileModel>.Filter;
            //BsonRegularExpression queryExpr = new BsonRegularExpression(new Regex($"^{request.query}.*", RegexOptions.IgnoreCase));

        }
    }
}
