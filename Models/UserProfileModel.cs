using MongoDB.Bson;
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

        public UserProfileModel()
        {
            ((List<LibraryItem>)this.forSale).Add(new LibraryItem { sampleId = "6330f644365c41309f73ec3c" });
        }

        public string accountId { get; set; } = "";
        public string avatarId { get; set; } = $"{Guid.NewGuid()}";
        public string customUserName { get; set; } = "";
        public List<LibraryItem> forSale { get; set; } = new List<LibraryItem>();
        public List<LibraryItem> owned { get; set; } = new List<LibraryItem>();
        public List<SampleModel> samples { get; set; } = new List<SampleModel>();
    }
}
