using System.Collections.Generic;

namespace vp.models
{
    public class Purchase : BaseModel
    {
        public string accountId { get; set; } = "";
        public string priceId { get; set; } = "";
        public Purchase() : base()
        {
        }
    }
}
