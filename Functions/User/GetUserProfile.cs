﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.DTO;
using vp.services;

namespace visiophone_cs_funcapp.Functions.User
{
    public class GetUserProfile
    {
        private readonly IUserService _userService;

        public GetUserProfile(IUserService userService)
        {
            _userService = userService;
        }

        [FunctionName("get_user_profile")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            UserProfileRequest request = JsonConvert.DeserializeObject<UserProfileRequest>(requestBody);
            return new OkObjectResult(await _userService.GetUserProfile(request));
        }
    }
}
