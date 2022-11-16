using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using vp.Models;
using Stripe;

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

        public bool isAuthenticated(ClaimsPrincipal principal, string targetId)
        {
            //TODO: FIX THIS!
            //GetAccountIdForToken(targetId);
            //var result = principal.Identities.Where(identity => identity.Claims.Any(claim => claim.Value == targetId));
            //return principal.Identity.IsAuthenticated;
            return true;
        }


        public UserProfileModel GetUserProfile(string accountId, bool throwNoExist = false)
        {
            var result =  _users
                .Find(u => u.accountId.Equals(UserProfileModel.GetAcccountIdFromToken(accountId)))
                .FirstOrDefault<UserProfileModel>();
            if(throwNoExist && result == null) throw new Exception($"failed to find user record for user: ${accountId}");
            return result;
        }

        public async Task<UserProfileModel> PurchaseSample(string accountId, string sampleId) {
            var userProfile = GetUserProfile(accountId, true);











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
            var userProfile = GetUserProfile(accountId, true);
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
            var lastProfile = GetUserProfile(userProfile.accountId);
            UserProfileModel forInsert;

            if (lastProfile == null)
            {
                forInsert = userProfile;
            }
            else
            {
                forInsert = lastProfile;
                forInsert.customUserName = userProfile.customUserName;
                forInsert.avatarId = userProfile.avatarId;
            }

            var filter = Builders<UserProfileModel>.Filter.Where(profile => profile.accountId == forInsert.accountId);
            await _users.ReplaceOneAsync(filter, forInsert, new ReplaceOptions { IsUpsert = true });

            return userProfile;
        }
    }
}
