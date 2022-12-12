﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using vp.util;
using Microsoft.AspNetCore.Mvc;

namespace vp.Functions.User
{
    public class SetUserProfile
    {
        private readonly IUserService _userService;

        public SetUserProfile(IUserService userService)
        {
            _userService = userService;
        }

        [FunctionName("set_user_profile")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!await _userService.AuthenticateUser(req, log))
            {
                return new UnauthorizedResult();
            }

            var userAccountId = _userService.GetUserAccountId(req.HttpContext.User);

            var meta = req.Form.Files[0];
            var contentType = req.Form.Files[0].ContentType;

            using (Stream stream = meta.OpenReadStream()) {
                Utils.UploadStream(stream, $"{userAccountId}.png", "avatars", contentType);
            }

            return new OkResult();
        }
    }

}
