﻿using System.Threading.Tasks;
using vp.DTO;
using vp.Models;

namespace vp.services
{
    public interface IUserService
    {
        UserProfileModel GetUserProfile(UserProfileRequest request);
        Task<UserProfileModel> SetUserProfile(UserProfileModel userProfile);
    }
}
