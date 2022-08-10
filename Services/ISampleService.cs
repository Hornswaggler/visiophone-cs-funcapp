using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public interface ISampleService
    {
        Task  AddSample(SampleModel sample);
        Task<SampleQueryResult> GetSamples(SampleRequest request);
    }
}
