using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using vp.services;
using vp.Models;
using System;
using vp.util;
using MongoDB.Bson.Serialization;

namespace visiophone_cs_funcapp.Functions.User
{
    public class SetUserProfile
    {
        private readonly IUserService _userService;

        public SetUserProfile(IUserService userService)
        {
            _userService = userService;
        }

        [FunctionName("set_user_profile")]
        public async Task<UserProfileModel> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var meta = req.Form.Files[0];
            var contentType = req.Form.Files[0].ContentType;
            UserProfileModel userProfile = BsonSerializer.Deserialize<UserProfileModel>(req.Form["data"]);
            userProfile.avatarId = $"{Guid.NewGuid()}";

            using (Stream stream = meta.OpenReadStream()) {
                Utils.UploadStream(stream, userProfile.avatarId + ".png", "avatars", contentType);
            }

            return await _userService.SetUserProfile(userProfile);
        }
    }

}
