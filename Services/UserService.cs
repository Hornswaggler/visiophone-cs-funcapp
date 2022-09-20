using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
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

        private string DEFAULT_ID = new ObjectId().ToString();

        public UserService(MongoClient mongoClient, IConfiguration configuration) {
            //TODO: move this up a layer...
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _users = _database.GetCollection<UserProfileModel>("users");
        }

        public UserProfileModel GetUserProfile(UserProfileRequest request)
        {
            UserProfileModel otherResult = _users.Find(u => u.accountId.Equals(request.userId)).FirstOrDefault<UserProfileModel>();


             return otherResult;

        }

        public async Task<UserProfileModel> SetUserProfile(UserProfileModel userProfile)
        {
            try
            {
                if (userProfile._id == null)
                {
                    await _users.InsertOneAsync(userProfile);
                    return userProfile;

                }
                else
                {
                    UserProfileModel result = await _users.FindOneAndReplaceAsync<UserProfileModel>(u => u._id.Equals(userProfile._id), userProfile);
                    return userProfile;
                }
            }
            catch (Exception e) {
                int i = 0;
                i++;
            }

            return userProfile;
        }
    }
}
