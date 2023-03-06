using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace vp.services
{
    public interface IUserService
    {
        Task<bool> AuthenticateUser(HttpRequest req);

        Task<Stripe.Account> AuthenticateSeller(HttpRequest req, ILogger log);
        string AuthenticateUserForm(HttpRequest req, ILogger log);
        string GetUserAccountId(ClaimsPrincipal claimsPrincipal);
    }
}
