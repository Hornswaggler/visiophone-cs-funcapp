using System.Threading.Tasks;
using vp.models;
using vp.functions.samplepack;

namespace vp.services
{
    public interface ISampleService
    {
        Task<Sample> AddSample(Sample sample);
        Task<SearchQueryResult<Sample>> GetSamplesByName(SearchQueryRequest request);
    }
}
