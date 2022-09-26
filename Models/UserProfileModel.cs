using System;
using vp.models;

namespace vp.Models
{
    public class UserProfileModel : BaseModel
    {
        public string accountId { get; set; } = "";
        public string avatarId { get; set; } = $"{Guid.NewGuid()}";
        public string customUserName { get; set; } = "";
    }
}
