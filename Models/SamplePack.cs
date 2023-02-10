
using System.Collections.Generic;

namespace vp.models
{
    public class SamplePack<T> : BaseModel where T : Sample
    {
        public SamplePack() : base() { }

        public SamplePack(string _id, string name, string description, List<T> samples) : base(_id) {
            this.name = name;
            this.description = description;
            this.samples = samples;
        }

        public List<T> samples { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}
