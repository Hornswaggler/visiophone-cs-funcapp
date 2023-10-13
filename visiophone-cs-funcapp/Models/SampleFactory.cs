using visiophone_cs_funcapp.Orchestrations.UpsertSamplePack;

namespace vp.models
{
    public class SampleFactory
    {
        public static Sample MakeSampleForSampleRequest(UpsertSampleRequest request)
        {
            return new Sample
            {
                id = request.id,
                name = request.name,
                tags = request.tags,
                key = request.key,
                description = request.description,
                bpm = request.bpm,
            };
        }
    }
}
