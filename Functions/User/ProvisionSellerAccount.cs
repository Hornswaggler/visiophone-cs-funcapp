using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using vp.DTO;

namespace vp.Functions.User
{
    public class ProvisionSellerAccount
    {

        internal class ProvisionSellerAccountDTO
        {
            public string stripeId { get; set; }
        }


        [FunctionName("provision_stripe_standard")]
        public HttpResponseMessage ProvisionStripeStandard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            StripeConfiguration.ApiKey = Config.StripeAPIKey;

            var options = new AccountCreateOptions { Type = "standard" };
            var service = new AccountService();
            var accountService = service.Create(options);

            var accountLink = CreateStripeStandardAccountLink(accountService.Id);


            //var context = req.HttpContext;
            //var cookieOptions = new CookieOptions()
            //{
            //    Path = "/",
            //    Expires = DateTimeOffset.UtcNow.AddHours(1),
            //    IsEssential = true,
            //    HttpOnly = false,
            //    Secure = false,
            //};
            //context.Response.Cookies.Append(
            //    "stripeId",
            //    accountService.Id,
            //    new CookieOptions()
            //    {
            //        Path = "/",
            //        Expires = DateTimeOffset.UtcNow.AddHours(1),
            //        IsEssential = true,
            //        HttpOnly = false,
            //        Secure = false,
            //    }
            //);




            //context.Crea

            var resp = new HttpResponseMessage();
            //resp.Headers.AddCookies(new CookieHeaderValue[]
            //{
            //    new CookieHeaderValue("stripeId", accountService.Id) {
            //        Expires = DateTimeOffset.Now.AddMinutes(5),
            //        HttpOnly = true,
            //        Path = "/"
            //    }
            //});
            resp.Content = new StringContent(JsonConvert.SerializeObject(
                new StripeSellerAccount
                {
                    id = accountService.Id,
                    url = accountLink.Url
                }
            ));

            return resp;
            //req.
            //resp.Content = JsonConvert.SerializeObject(
            //    new StripeSellerAccount { 
            //        url = accountLink.Url
            //    }
            //)

            //var result = new StripeSellerAccount()
            //{
            //    url = accountLink.Url
            //};


            //Response accountService.Id

            //return res;
        }

        [FunctionName("provision_stripe_standard_return")]
        public async Task<string> ProvisionStripeStandardReturn(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = null)] HttpRequest req,
            ILogger log, ClaimsPrincipal principal
        )
        {
            //log.LogInformation("Processing string standard return");
            log.LogInformation("Processing string standard response");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var seller = JsonConvert.DeserializeObject<ProvisionSellerAccountDTO>(requestBody);
            //account.stripeId

            //StripeConfiguration.ApiKey = "sk_test_YLa694rQaWsXLKPrPJQ0bvF6";

            StripeConfiguration.ApiKey = Config.StripeAPIKey;
            var service = new AccountService();
            var account = service.Get(seller.stripeId);

            //Persist stripeId


            return null;
        }

        [FunctionName("provision_stripe_standard_refresh")]
        public string ProvisionStripeStandardRefresh(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log
        )
        {
            //log.LogInformation("Processing string standard response");
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //var request = JsonConvert.DeserializeObject<ProvisionSellerAccountDTO>(requestBody);


            return null;
        }
        public AccountLink CreateStripeStandardAccountLink(string accountId) {
            var options = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = Config.ProvisionStripeStandardRefreshUrl,
                ReturnUrl = Config.ProvisionStripeStandardReturnUrl,
                Type = "account_onboarding",
            };
            var service = new AccountLinkService();
            return service.Create(options);
            
        }
    }
}
