using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public interface ISampleService
    {
        Task<Sample> AddSample(Sample sample);
        Task<SearchQueryResult<Sample>> GetSamplesByName(SearchQuery request);
        Task<List<Sample>> GetSamples(List<string> priceIds);
    }
}
