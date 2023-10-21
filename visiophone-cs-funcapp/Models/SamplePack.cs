
using System.Collections.Generic;
using System.Linq;
using vp.orchestrations.upsertSamplePack;

namespace vp.models
{
    public class SamplePack<T> : BaseModel where T : Sample
    {
        public SamplePack() : base() { }

        public SamplePack(string id, string name, string description, List<T> samples) : base(id) {
            this.name = name;
            this.description = description;
            this.samples = samples;
        }

        public List<T> samples { get; set; } = new List<T>();
        public string name { get; set; }
        public string description { get; set; }
        public string priceId { get; set; }
        public string productId { get; set; }
        public decimal? cost { get; set; } = 0;
        public string sellerId { get; set; } = "";
        public string seller { get; set; } = "";

        public static explicit operator SamplePack<T>(UpsertSamplePackRequest v)
        {
            var result = new SamplePack<T>();
            result.id = v.id;
            result.priceId = v.priceId;
            result.productId = v.productId;
            result.samples = (List<T>)v.samples.Select(sample => sample as T).ToList();
            result.name = v.name;
            result.description = v.description;
            result.cost = v.cost;
            result.sellerId = v.sellerId;
            result.seller = v.seller;

            return result;
        }

        //TODO: Aggregate what's in pack? or search within collection records?
        // SHOULD BE ABLE TO LEVERAGE TREE BASED INDICES INSTEAD OF HASHMAP INDICES!!! 
        //public List<string> tags { get; set; } = new List<string>();
        //public string keys { get; set; } = "";
    }
}
