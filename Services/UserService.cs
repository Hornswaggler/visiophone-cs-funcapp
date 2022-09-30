using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;
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

        public string GetAccountIdForToken(string accountId) {
            return accountId.Split('.')[1];
        }

        public bool isAuthenticated(ClaimsPrincipal principal, string targetId)
        {
            //TODO: FIX THIS!
            //GetAccountIdForToken(targetId);
            //var result = principal.Identities.Where(identity => identity.Claims.Any(claim => claim.Value == targetId));
            //return principal.Identity.IsAuthenticated;
            return true;
        }


        public UserProfileModel GetUserProfile(UserProfileRequest request)
        {
            return _users
                .Find(u => u.accountId.Equals(GetAccountIdForToken(request.accountId)))
                .FirstOrDefault<UserProfileModel>();
        }

        public async Task<UserProfileModel> PurchaseSample(string accountId, string sampleId) {
            UserProfileModel userProfile = _users
                .Find(u => u.accountId.Equals(GetAccountIdForToken(accountId)))
                .FirstOrDefault<UserProfileModel>();

            if (userProfile == null)
            {
                throw new Exception($"failed to find user record for user: ${accountId}");
            }

            userProfile.owned.Add(new UserProfileModel.LibraryItem
            {
                sampleId = sampleId,
            });

            var filter = Builders<UserProfileModel>.Filter.Where(profile => profile.accountId == userProfile.accountId);
            await _users.ReplaceOneAsync(filter, userProfile, new ReplaceOptions { IsUpsert = true });

            return userProfile;
        }

        public async Task<UserProfileModel> AddForSale(string accountId, string sampleId)
        {
            UserProfileModel userProfile = _users
                .Find(u => u.accountId.Equals(GetAccountIdForToken(accountId)))
                .FirstOrDefault<UserProfileModel>();

            if (userProfile == null)
            {
                throw new Exception($"failed to find user record for user: ${accountId}");
            }

            userProfile.forSale.Add(new UserProfileModel.LibraryItem
            {
                sampleId = sampleId
            });

            var filter = Builders<UserProfileModel>.Filter.Where(profile => profile.accountId == userProfile.accountId);
            await _users.ReplaceOneAsync(filter, userProfile, new ReplaceOptions { IsUpsert = true });

            return userProfile;
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
