using System;
using System.Collections.Generic;
using vp.models;

namespace vp.Models
{
    public class UserProfile : BaseModel
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

        public UserProfile() {
        }

        public List<LibraryItem> forSale { get; set; } = new List<LibraryItem>();
        public List<LibraryItem> owned { get; set; } = new List<LibraryItem>();
        public List<Sample> samples { get; set; } = new List<Sample>();
    }
}
