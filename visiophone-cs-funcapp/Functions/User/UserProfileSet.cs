using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using Microsoft.AspNetCore.Mvc;

namespace vp.functions.user
{
    public class UserProfileSet
    {
        private readonly IUserService _userService;
        private readonly IStorageService _storageService;

        public UserProfileSet(IUserService userService, IStorageService storageService)
        {
            _userService = userService;
            _storageService = storageService;
        }

        [FunctionName(FunctionNames.UserProfileSet)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await _userService.AuthenticateUser(req))
            {
                return new UnauthorizedResult();
            }

            var userAccountId = _userService.GetUserAccountId(req.HttpContext.User);
            _storageService.UploadUserAvatar(req.Form.Files[0], $"{userAccountId}.png");

            return new OkResult();
        }
    }

}
