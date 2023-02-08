using vp.orchestrations.upsertsample;

namespace vp.models
{
    public class SampleFactory
    {
        public static Sample MakeSampleForSampleRequest(UpsertSampleRequest request)
        {
            return new Sample
            {
                _id = request._id,
                name = request.name,
                tag = request.tag,
                description = request.description,
                seller = request.seller,
                bpm = request.bpm,
                cost = request.cost,
                priceId = request.priceId,
                sellerId = request.sellerId
            };
        }
    }
}
