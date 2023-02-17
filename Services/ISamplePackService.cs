using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public interface ISamplePackService
    {
        Task<SamplePack<Sample>> AddSamplePack(SamplePack<Sample> samplePack);
        Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksByName(SearchQuery request);
        Task<SamplePack<Sample>> GetSamplePackById(string samplePackId);
    }
}
