using System;
using System.Collections.Generic;
using vp.models;

namespace vp.Models
{
    public class UserProfileModel : BaseModel
    {

        public class LibraryItem
        {
            public string sampleId { get; set; }
        }

        public static string GetAcccountIdFromToken(string accountId) {
            if (accountId != null && accountId.Contains("."))
            {
                return accountId.Substring(accountId.IndexOf('.') + 1);
            }
            return accountId;
        }

        public UserProfileModel() {
            accountId = GetAcccountIdFromToken(accountId);
        }

        public string accountId { get; set; }
        public string stripeId { get; set; }
        public string avatarId { get; set; } = $"{Guid.NewGuid()}";
        public string customUserName { get; set; } = "";
        public List<LibraryItem> forSale { get; set; } = new List<LibraryItem>();
        public List<LibraryItem> owned { get; set; } = new List<LibraryItem>();
        public List<SampleModel> samples { get; set; } = new List<SampleModel>();
    }
}
