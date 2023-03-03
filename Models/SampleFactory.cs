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
                tags = request.tags,
                key = request.key,
                description = request.description,
                bpm = request.bpm,
            };
        }
    }
}
