
using System.Collections.Generic;

namespace vp.models
{
    public class SamplePack : BaseModel
    {
        public SamplePack() : base() { }

        public SamplePack(string _id, string name, string description, List<Sample> samples) : base(_id) {
            this.name = name;
            this.description = description;
            this.samples = samples;
        }

        public List<Sample> samples { get; set; } = new List<Sample>();
        public string name { get; set; }
        public string description { get; set; }
    }
}
