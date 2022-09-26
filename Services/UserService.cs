using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using vp.DTO;
using vp.Models;

namespace vp.services
{
    public class UserService : IUserService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<UserProfileModel> _users;

        private string DEFAULT_ID = new ObjectId().ToString();

        public UserService(MongoClient mongoClient, IConfiguration configuration) {
            //TODO: move this up a layer...
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _users = _database.GetCollection<UserProfileModel>("users");
        }

        public UserProfileModel GetUserProfile(UserProfileRequest request)
        {
            string accountId = request.userId.Split('.')[1];
            UserProfileModel otherResult = _users.Find(u => u.accountId.Equals(accountId)).FirstOrDefault<UserProfileModel>();

             return otherResult;
        }

        public async Task<UserProfileModel> SetUserProfile(UserProfileModel userProfile)
        {
            if (userProfile.accountId.Contains("."))
            {
                userProfile.accountId = userProfile.accountId.Split('.')[1];

            }

            var filter = Builders<UserProfileModel>.Filter.Where(profile => profile.accountId == userProfile.accountId);
            await _users.ReplaceOneAsync(filter, userProfile, new ReplaceOptions { IsUpsert = true });

            return userProfile;
        }
    }
}
