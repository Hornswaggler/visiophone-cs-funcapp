using vp.orchestrations;

namespace vp.models
{
    public class Sample : BaseModel
    {
        public Sample() : base() { }

        public Sample(UpsertSampleRequest request) : this(
            request._id,
            request.name,
            request.tag,
            request.description,
            request.seller,
            request.bpm,
            request.cost,
            request.priceId,
            request.sellerId
        )
        { }

        public Sample(
            string _id, 
            string name, 
            string tag, 
            string description, 
            string seller, 
            string bpm, 
            decimal? cost, 
            string priceId, 
            string sellerId) : base(_id)
        {
            this.name = name;
            this.tag = tag;
            this.description = description;
            this.seller = seller;
            this.bpm = bpm;
            this.cost = cost;
            this.priceId = priceId;
            this.sellerId = sellerId;
        }

        public string name { get; set; } = "";
        public string tag { get; set; } = "";
        public string description { get; set; } = "";
        public string seller { get; set; } = "";
        public string bpm { get; set; } = "";
        public decimal? cost { get; set; } = 0;
        public string priceId { get; set; } = "";
        public string sellerId { get; set; } = "";
    }
}
