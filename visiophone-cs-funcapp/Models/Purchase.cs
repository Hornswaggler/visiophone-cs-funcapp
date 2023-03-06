using System.Collections.Generic;

namespace vp.models
{
    public class Purchase : BaseModel
    {
        public string accountId { get; set; } = "";
        public string priceId { get; set; } = "";

        //TODO: Validation (SAMPLE / SAMPLE_PACK)
        public string type { get; set; } = "";
        public Purchase() : base()
        {
        }
    }
}
