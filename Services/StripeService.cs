using MongoDB.Driver;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public class StripeService : IStripeService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<StripeProfile> _profileCollection;
        private readonly AccountService _accountService;
        private readonly AccountLinkService _linkService;
        private readonly ProductService _productService;
        private readonly SessionService _sessionService;

        public StripeService(MongoClient mongoClient)
        {
            StripeConfiguration.ApiKey = Config.StripeAPIKey;
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _accountService = new AccountService();
            _productService = new ProductService();
            _linkService = new AccountLinkService();
            _sessionService = new SessionService();
            _profileCollection = _database.GetCollection<StripeProfile>("stripeProfiles");
        }

        public Session CreateSession(string accountId, List<string> priceIds) {
            var lineItems = new List<SessionLineItemOptions>();
            foreach(var priceId in priceIds)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                });
            }

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Config.PurchaseSampleStripeReturnUrl,
                CancelUrl = Config.PurchaseSampleStripeCancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "vp_accountId", accountId }
                }
            };

            Session session = _sessionService.Create(options);

            return session;
        }

        public async Task<Account> GetStripeAccount(StripeProfile stripeProfile) {
            return await _accountService.GetAsync(stripeProfile.stripeId);
        }

        public StripeProfile GetStripeProfile(string accountId, bool throwNoExist = false) {
            StripeProfile profile = _profileCollection.Find(u => u.accountId.Equals(accountId)).FirstOrDefault();
            
            if (throwNoExist && profile == null) throw new Exception($"failed to find stripe account record for user: ${accountId}");
            else if(profile == null)
            {
                return null;
            }

            StripeProfileDTO result = new StripeProfileDTO
            {
                accountId = profile.accountId,
                stripeId = profile.stripeId,
                isStripeApproved = profile.isStripeApproved,
            };

            return profile;
        }

        public async Task<StripeProfile> CreateNewAccount(string accountId)
        {
            //TODO: Get the email from the identity token
            var options = new AccountCreateOptions
            {
                Type = "custom",
                Country = "US",
                Email = "founders@visiophone.wtf",
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                },
            };

            var stripeAccount = await _accountService.CreateAsync(options);

            return await SetStripeProfile(new StripeProfile
            {
                accountId = accountId,
                stripeId = stripeAccount.Id,
            });
        }

        public async Task<AccountLink> CreateAccountLink(string stripeId) {
            return await _linkService.CreateAsync(new AccountLinkCreateOptions
            {
                Account = stripeId,
                RefreshUrl = Config.ProvisionStripeStandardRefreshUrl,
                ReturnUrl = Config.ProvisionStripeStandardReturnUrl,
                Type = "account_onboarding",
            });
        }

        public async Task<StripeProfile> SetStripeProfile(StripeProfile stripeProfile) {
            StripeProfile lastProfile = GetStripeProfile(stripeProfile.accountId);

            var filter = Builders<StripeProfile>.Filter.Where(profile => profile.accountId == stripeProfile.accountId);
            await _profileCollection.ReplaceOneAsync(filter, stripeProfile, new ReplaceOptions { IsUpsert = true });

            return stripeProfile;
        }

        public Session GetCheckoutSession(string sessionId) {
            return _sessionService.Get(sessionId);
        }

        public List<string> GetPriceIdsForSession(string sessionId)
        {
            var options = new SessionListLineItemsOptions
            {
                Limit = 100,
            };

            var priceIds = new List<string>();
            var hasMore = true;
            while (hasMore)
            {
                StripeList<LineItem> lineItems = _sessionService.ListLineItems(sessionId, options);
                foreach (var lineItem in lineItems.Data)
                {
                    priceIds.Add(lineItem.Price.Id);
                }
                hasMore = lineItems.HasMore;
            }

            return priceIds;
        }

        public List<Sample> GetProductsForUser(string stripeId)
        {
            var options = new ProductSearchOptions
            {
                Query = $"active:'true' AND metadata['accountId']:'{stripeId}'",
            };
            var result = _productService.Search(options);
            var products = result.Data as List<Product>;

            List<Sample> samples = new List<Sample>();
            foreach(Product product in products)
            {
                samples.Add(new Sample
                {
                    name = product.Name,
                    description = product.Description,
                });
            }

            return samples;
        }
    }
}
