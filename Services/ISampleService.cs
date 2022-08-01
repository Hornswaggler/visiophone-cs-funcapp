using System.Collections.Generic;
using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public interface ISampleService
    {
        Task  AddSample(SampleRequest sample);
        Task<List<SampleRequest>> GetSamples(SampleRequest page);

    }
}

