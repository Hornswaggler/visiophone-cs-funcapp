
using System.Collections.Generic;

namespace vp.DTO
{
    public class SamplePurchaseRequest
    {
        public List<string> priceIds { get; set; } = new List<string>();
        public string accountId { get; set; } = "";
    }
}
