using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using vp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;
using System.Net;
using Microsoft.Extensions.Logging;

namespace vp.services
{
    public class UserService : IUserService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<UserProfile> _users;

        public UserService(MongoClient mongoClient, IConfiguration configuration) {
            //TODO: move this up a layer...
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _users = _database.GetCollection<UserProfile>("users");
        }

        public async Task<bool> AuthenticateUser(HttpRequest req, ILogger log) {
            (bool authenticationStatus, IActionResult authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();

            if (!authenticationStatus)
            {
                try
                {
                    ObjectResult objectResult = (ObjectResult)authenticationResponse;
                    string errorResponse = ((ProblemDetails)objectResult.Value).Detail;
                    log.LogWarning(errorResponse);
                }
                catch (Exception)
                {
                    //consume
                }
                
                return false;
            }

            return true;
        }


        public string GetUserAccountId(HttpRequest req) {
            var user = req.HttpContext.User;

            if (!user.HasClaim(Config.AuthClaimSignInAuthority, Config.AuthSignInAuthority))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            return user.FindFirst(Config.AuthClaimId).Value;
        }

        public UserProfile GetUserProfile(string accountId, bool throwNoExist = false)
        {
            //var result =  _users
            //    .Find(u => u.accountId.Equals(UserProfile.GetAcccountIdFromToken(accountId)))
            //    .FirstOrDefault<UserProfile>();
            //if(throwNoExist && result == null) throw new Exception($"failed to find user record for user: ${accountId}");
            //return result;
            return null;
        }

        public async Task<UserProfile> PurchaseSample(string accountId, string sampleId) {
            //var userProfile = GetUserProfile(accountId, true);

            //userProfile.owned.Add(new UserProfile.LibraryItem
            //{
            //    sampleId = sampleId,
            //});

            //var filter = Builders<UserProfile>.Filter.Where(profile => profile.accountId == userProfile.accountId);
            //await _users.ReplaceOneAsync(filter, userProfile, new ReplaceOptions { IsUpsert = true });

            //return userProfile;
            return null;
        }

        public async Task<UserProfile> AddForSale(string accountId, string sampleId)
        {
            //var userProfile = GetUserProfile(accountId, true);
            //userProfile.forSale.Add(new UserProfile.LibraryItem
            //{
            //    sampleId = sampleId
            //});

            //var filter = Builders<UserProfile>.Filter.Where(profile => profile.accountId == userProfile.accountId);
            //await _users.ReplaceOneAsync(filter, userProfile, new ReplaceOptions { IsUpsert = true });

            //return userProfile;
            return null;
        }

        public async Task<UserProfile> SetUserProfile(UserProfile userProfile)
        {
            //UserProfile lastProfile = GetUserProfile(userProfile.accountId);
            //UserProfile forInsert;

            //if (lastProfile == null)
            //{
            //    forInsert = userProfile;
            //}
            //else
            //{
            //    forInsert = lastProfile;
            //    forInsert.customUserName = userProfile.customUserName;
            //    forInsert.avatarId = userProfile.avatarId;
            //}

            //var filter = Builders<UserProfile>.Filter.Where(profile => profile.accountId == forInsert.accountId);
            //await _users.ReplaceOneAsync(filter, forInsert, new ReplaceOptions { IsUpsert = true });

            //return userProfile;
            return null;
        }
    }
}
