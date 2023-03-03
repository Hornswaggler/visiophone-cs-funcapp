using Microsoft.Azure.Cosmos;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.functions.stripe;
using vp.models;

namespace vp.services
{
    public class StripeService : BaseEntity<StripeProfile>, IStripeService
    {
        private readonly Container _stripeProfilesCosmos;
        private readonly AccountService _accountService;
        private readonly AccountLinkService _linkService;
        private readonly ProductService _productService;
        private readonly SessionService _sessionService;

        public StripeService(CosmosClient cosmosClient) : base(cosmosClient)
        {
            StripeConfiguration.ApiKey = Config.StripeAPIKey;
            _accountService = new AccountService();
            _productService = new ProductService();
            _linkService = new AccountLinkService();
            _sessionService = new SessionService();
            _stripeProfilesCosmos = _cosmosClient.GetContainer(Config.DatabaseName, Config.StripeProfileCollectionName);
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

        public async Task<string> GetAccountLink(StripeProfile profile, string returnUri)
        {
            var accountLink = await CreateAccountLink(profile.stripeId, returnUri);
            return accountLink.Url;
        }

        public async Task<StripeProfileResult> GetStripeProfile(string accountId, bool throwNoExist = false) {
            StripeProfile profile = await GetByField(_stripeProfilesCosmos, "accountId", accountId);

            if (throwNoExist && profile == null) throw new Exception($"failed to find stripe account record for user: ${accountId}");
            else if(profile == null)
            {
                return null;
            }

            var account = await GetStripeAccount(profile);
            return new StripeProfileResult
            {
                accountId = profile.accountId,
                stripeId = profile.stripeId,
                isStripeApproved = account.DetailsSubmitted,
                defaultCurrency = account.DefaultCurrency
            };
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
                Metadata = new Dictionary<string, string>
                {
                    {"visiophone_accountid" , accountId }
                }
            };

            var stripeAccount = await _accountService.CreateAsync(options);

            var profile = new StripeProfile
            {
                id = Guid.NewGuid().ToString(),
                accountId = accountId,
                stripeId = stripeAccount.Id,
            };

            await _stripeProfilesCosmos.UpsertItemAsync(profile);

            return profile;
        }

        public async Task<AccountLink> CreateAccountLink(string stripeId, string returnUri) {
            return await _linkService.CreateAsync(new AccountLinkCreateOptions
            {
                Account = stripeId,
                RefreshUrl = returnUri,
                ReturnUrl = returnUri,
                Type = "account_onboarding",
            });
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

        public List<Product> GetProductsForUser(string stripeId)
        {
            var options = new ProductSearchOptions
            {
                Query = $"active:'true' AND metadata['accountId']:'{stripeId}'",
            };
            var result = _productService.Search(options);
            return result.Data;
        }
    }
}
