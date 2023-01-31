using System.Collections.Generic;
using System.Threading.Tasks;
using vp.DTO;
using vp.models;

namespace vp.services
{
    public interface ISamplePackService
    {
        Task<SamplePack> AddSamplePack(SamplePack samplePack);
    }
}
