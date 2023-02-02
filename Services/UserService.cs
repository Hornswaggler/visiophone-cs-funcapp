using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace vp.services
{
    public class UserService : IUserService
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IStripeService _stripeService;

        public UserService(MongoClient mongoClient, IConfiguration configuration, IStripeService stripeService)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("visiophone");
            _stripeService = stripeService;
        }

        public async Task<bool> AuthenticateUser(HttpRequest req)
        {
            (bool authenticationStatus, IActionResult authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();

            if (!authenticationStatus)
            {
                try
                {
                    ObjectResult objectResult = (ObjectResult)authenticationResponse;
                    string errorResponse = ((ProblemDetails)objectResult.Value).Detail;
                }
                catch (Exception)
                {
                    //consume
                }

                return false;
            }

            return true;
        }

        public string AuthenticateUserForm(HttpRequest req, ILogger log)
        {
            string idToken = req.Form["idToken"].ToString();
            ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();

            SecurityToken securityToken;
            var claimsPrincipal = tokenValidator.ValidateToken(idToken, VPTokenValidationParamters.tokenValidationParameters, out securityToken);

            if (!claimsPrincipal.Identity.IsAuthenticated
                || !claimsPrincipal.HasClaim(Config.AuthClaimSignInAuthority, Config.AuthSignInAuthority))
            {
                throw new UnauthorizedAccessException();
            }

            string accountId = GetUserAccountId(claimsPrincipal);
            if (accountId != null && accountId.Trim().Equals(""))
            {
                throw new UnauthorizedAccessException();
            }


            return accountId;
        }

        public async Task<Stripe.Account> AuthenticateSeller(HttpRequest req, ILogger log) {

            if (!await AuthenticateUser(req))
            {
                return null;
            }

            var stripeProfile = _stripeService.GetStripeProfile(GetUserAccountId(req.HttpContext.User));
            if (stripeProfile == null)
            {
                return null;
            }

            Stripe.Account stripeAccount = await _stripeService.GetStripeAccount(stripeProfile);
            if (!stripeAccount.DetailsSubmitted)
            {
                return null;
            }

            return stripeAccount;
        }

        public string GetUserAccountId(ClaimsPrincipal claimsPrincipal) {
            if (!claimsPrincipal.HasClaim(Config.AuthClaimSignInAuthority, Config.AuthSignInAuthority))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            return claimsPrincipal.FindFirst(Config.AuthClaimId).Value;
        }
    }
}
