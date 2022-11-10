using System.Security.Claims;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;
using vp.Models;

namespace vp.services
{
    public interface IUserService
    {
        UserProfileModel GetUserProfile(string accountId, bool throwNoExist = false);
        Task<UserProfileModel> SetUserProfile(UserProfileModel userProfile);
        bool isAuthenticated(ClaimsPrincipal principal, string targetId);
        Task<UserProfileModel> PurchaseSample(string accountId, string sampleId);
        Task<UserProfileModel> AddForSale(string accountId, string sampleId);
    }
}
