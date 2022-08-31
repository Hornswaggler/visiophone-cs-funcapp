using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.services;
using vp.Models;
using System;

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
            //var meta = req.Form.Files[0];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //UserProfileModel userProfile = JsonConvert.DeserializeObject<UserProfileModel>(req.Form["data"]);
            await new StreamReader(req.Body).ReadToEndAsync();
            UserProfileModel userProfile = JsonConvert.DeserializeObject<UserProfileModel>(requestBody);

            userProfile.avatarId = $"{Guid.NewGuid()}";

            UserProfileModel result = await _userService.SetUserProfile(userProfile);

            //string extension = meta.FileName.IndexOf('.') != -1 ? ".wav" : meta.FileName.Split('.')[0];

            return result;
        }
    }

}
