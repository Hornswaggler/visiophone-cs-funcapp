using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using vp.Models;

namespace vp.services
{
    public interface IUserService
    {
        UserProfile GetUserProfile(string accountId, bool throwNoExist = false);
        Task<UserProfile> SetUserProfile(UserProfile userProfile);
        Task<UserProfile> PurchaseSample(string accountId, string sampleId);
        Task<UserProfile> AddForSale(string accountId, string sampleId);

        Task<bool> AuthenticateUser(HttpRequest req, ILogger log);
        string GetUserAccountId(HttpRequest req);
    }
}
