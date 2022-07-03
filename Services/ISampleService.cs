using System.Collections.Generic;
using System.Threading.Tasks;

namespace vp
{
    public interface ISampleService
    {
        Task  AddSample(SampleModel sample);
        Task<List<SampleModel>> GetSamples();

    }
}

