using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public interface ISampleService
    {
        Task<SampleModel>  AddSample(SampleModel sample);
        Task<SampleQueryResult> GetSamples(SampleRequest request);
        Task<SampleModel> GetSampleById(string id);
        Task<List<SampleModel>> GetSamplesById(IEnumerable<string> sampleIds);
    }
}
