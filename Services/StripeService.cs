using MongoDB.Driver;
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
        private readonly IMongoCollection<StripeProfile> _profiles;
        private readonly Stripe.AccountService _accountService;
        private readonly Stripe.AccountLinkService _linkService;
        private readonly Stripe.ProductService _productService;

        public StripeService(MongoClient mongoClient)
        {
            Stripe.StripeConfiguration.ApiKey = Config.StripeAPIKey;
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _accountService = new Stripe.AccountService();
            _productService = new Stripe.ProductService();
            _linkService = new Stripe.AccountLinkService();
            _profiles = _database.GetCollection<StripeProfile>("stripeProfiles");
        }

        public Session CreateSession(SamplePurchaseRequest purchaseRequest) {
            var lineItems = new List<SessionLineItemOptions>();
            foreach(var sample in purchaseRequest.samples)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    Price = sample.priceId,
                    Quantity = 1
                });
            }

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Config.PurchaseSampleStripeReturnUrl,
                CancelUrl = Config.PurchaseSampleStripeCancelUrl,
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return session;
        }

        public async Task<Stripe.Account> GetStripeAccount(StripeProfile stripeProfile) {


            //var options = new Stripe.ProductListOptions
            //{
            //    Limit = 3,
            //};
            //var service = new Stripe.ProductService();
            //Stripe.StripeList<Stripe.Product> products = service.List(
            //  options);


            //var options = new SessionCreateOptions
            //{
            //    LineItems = new List<SessionLineItemOptions>
            //    {
            //    new SessionLineItemOptions
            //    {
            //        Price = "price_1MAisxPILx3YgDycRLexHv3q",
            //        Quantity = 1,
            //    },
            //    },
            //    Mode = "payment",
            //    SuccessUrl = "https://example.com/success",
            //    CancelUrl = "https://example.com/cancel",
            //    PaymentIntentData = new SessionPaymentIntentDataOptions
            //    {
            //        ApplicationFeeAmount = 123,
            //    },
            //};

            //var requestOptions = new Stripe.RequestOptions
            //{
            //    StripeAccount = "acct_1MAiFGPILx3YgDyc",
            //};
            //var service = new SessionService();
            //Session session = service.Create(options, requestOptions);


            return await _accountService.GetAsync(stripeProfile.stripeId);
        }

        public StripeProfile GetStripeProfile(string accountId, bool throwNoExist = false) {
            StripeProfile profile = _profiles.Find(u => u.accountId.Equals(accountId)).FirstOrDefault();
            if (throwNoExist && profile == null) throw new Exception($"failed to find stripe account record for user: ${accountId}");
            return profile;
        }

        public void AddProduct(string name) {

            var options = new Stripe.ProductCreateOptions
            {
                Name = name,
            };

            //_productService.Create();

        }

        public async Task<StripeProfile> CreateNewAccount(string accountId)
        {

            //TODO: Get the email from the identity token
            var options = new Stripe.AccountCreateOptions
            {
                Type = "custom",
                Country = "US",
                Email = "founders@visiophone.wtf",
                Capabilities = new Stripe.AccountCapabilitiesOptions
                {
                    CardPayments = new Stripe.AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new Stripe.AccountCapabilitiesTransfersOptions
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

        public async Task<Stripe.AccountLink> CreateAccountLink(string stripeId) {
            return await _linkService.CreateAsync(new Stripe.AccountLinkCreateOptions
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
            await _profiles.ReplaceOneAsync(filter, stripeProfile, new ReplaceOptions { IsUpsert = true });

            return stripeProfile;
        }

    }
}
