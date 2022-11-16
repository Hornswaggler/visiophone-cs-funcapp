using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.DTO;
using vp.services;
using vp.Models;
using System.Linq;

namespace vp.Functions.User
{
    public class GetUserProfile
    {
        private readonly IUserService _userService;
        private readonly ISampleService _sampleService;

        public GetUserProfile(IUserService userService, ISampleService sampleService)
        {
            _userService = userService;
            _sampleService = sampleService;
        }

        [FunctionName("get_user_profile")]
        public async Task<UserProfileModel> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //TODO: Add auth check here (check contents w/ header to ensure they match)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserProfileRequest request = JsonConvert.DeserializeObject<UserProfileRequest>(requestBody);
            var userProfile = _userService.GetUserProfile(request.accountId, true);

            userProfile.samples.AddRange(
                await _sampleService.GetSamplesById(
                    userProfile.forSale
                        .Select(libraryItem => libraryItem.sampleId)
                        .Union(userProfile.owned
                            .Select(libraryItem => libraryItem.sampleId)
                        )
                )
            );

            return userProfile;
        }
    }
}
