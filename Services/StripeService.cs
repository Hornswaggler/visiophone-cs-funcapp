using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public class StripeService : IStripeService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<StripeProfile> _profiles;
        private readonly Stripe.AccountService _accountService;
        private readonly Stripe.AccountLinkService _linkService;


        public StripeService(MongoClient mongoClient, IConfiguration configuration)
        {
            Stripe.StripeConfiguration.ApiKey = Config.StripeAPIKey;
            //TODO: move this up a layer...
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _accountService = new Stripe.AccountService();
            _linkService = new Stripe.AccountLinkService();

            //TODO Make this into a provisioning script to be run in the deployment pipeline....This will provision the Sample collection(specifically for local dev)
            //var bson = new BsonDocument
            //{
            //    { "customAction", "CreateCollection" },
            //    { "collection", "stripeProfiles" },//update CollectionName
            //    { "shardKey", "key" }, //update ShardKey
            //    { "offerThroughput", 400} //update Throughput
            //};
            //var shellCommand = new BsonDocumentCommand<BsonDocument>(bson);
            //_database.RunCommand(shellCommand);

            _profiles = _database.GetCollection<StripeProfile>("stripeProfiles");
        }

        public async Task<Stripe.Account> GetStripeAccount(StripeProfile stripeProfile) {
            return await _accountService.GetAsync(stripeProfile.stripeId);
        }

        public StripeProfile GetStripeProfile(string accountId, bool throwNoExist = false) {
            StripeProfile profile = _profiles.Find(u => u.accountId.Equals(accountId)).FirstOrDefault();
            if (throwNoExist && profile == null) throw new Exception($"failed to find stripe account record for user: ${accountId}");
            return profile;
        }


        public async Task<StripeProfile> CreateNewAccount(string accountId)
        {
            var options = new Stripe.AccountCreateOptions { Type = "standard" };
            var stripeAccount = await _accountService.CreateAsync(options);

            var accountLink = await _linkService.CreateAsync(new Stripe.AccountLinkCreateOptions
            {
                Account = stripeAccount.Id,
                RefreshUrl = Config.ProvisionStripeStandardRefreshUrl,
                ReturnUrl = Config.ProvisionStripeStandardReturnUrl,
                Type = "account_onboarding",
            });

            var newProfile = new StripeProfile
            {
                accountId = accountId,
                stripeId = stripeAccount.Id,
                stripeUri = accountLink.Url
            };

            return await SetStripeProfile(newProfile);
        }

        public async Task<StripeProfile> SetStripeProfile(StripeProfile stripeProfile) {
            StripeProfile lastProfile = GetStripeProfile(stripeProfile.accountId);

            var filter = Builders<StripeProfile>.Filter.Where(profile => profile.accountId == stripeProfile.accountId);
            await _profiles.ReplaceOneAsync(filter, stripeProfile, new ReplaceOptions { IsUpsert = true });

            return stripeProfile;
        }

    }
}
