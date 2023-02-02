namespace vp.models
{
    public class Sample : BaseModel
    {
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
