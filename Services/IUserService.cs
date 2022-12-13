using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using vp.Models;

namespace vp.services
{
    public interface IUserService
    {
        Task<bool> AuthenticateUser(HttpRequest req, ILogger log);
        Task<Stripe.Account> AuthenticateSeller(HttpRequest req, ILogger log);
        UserProfile GetUserProfile(string accountId, bool throwNoExist = false);
        string AuthenticateUserForm(HttpRequest req, ILogger log);
        string GetUserAccountId(ClaimsPrincipal claimsPrincipal);
    }
}
