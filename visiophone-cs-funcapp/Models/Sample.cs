using System.Collections.Generic;

namespace vp.models
{
    public class Sample : BaseModel
    {
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public string bpm { get; set; } = "";
        //TODO: Enum
        public string key { get; set; } = "";
        //TODO: Enum
        public List<string> tags { get; set; } = new List<string>();
    }
}
