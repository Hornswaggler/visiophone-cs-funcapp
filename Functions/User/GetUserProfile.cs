using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using Microsoft.AspNetCore.Mvc;

namespace vp.Functions.User
{
    public class GetUserProfile
    {
        private readonly IUserService _userService;
        private readonly ISampleService _sampleService;
        private readonly IStripeService _stripeService;

        public GetUserProfile(IUserService userService, ISampleService sampleService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
            _sampleService = sampleService;
            _stripeService = stripeService;
        }

        [FunctionName("get_user_profile")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await _userService.AuthenticateUser(req, log))
            {
                return new UnauthorizedResult();
            }

            var stripeProfile = _stripeService.GetStripeProfile(_userService.GetUserAccountId(req));

            //TODO: Add auth check here (check contents w/ header to ensure they match)
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //UserProfileRequest request = JsonConvert.DeserializeObject<UserProfileRequest>(requestBody);
            //var userProfile = _userService.GetUserProfile(request.accountId, true);

            //userProfile.samples.AddRange(
            //    await _sampleService.GetSamplesById(
            //        userProfile.forSale
            //            .Select(libraryItem => libraryItem.sampleId)
            //            .Union(userProfile.owned
            //                .Select(libraryItem => libraryItem.sampleId)
            //            )
            //    )
            //);

            return new OkObjectResult(stripeProfile);
        }
    }
}
