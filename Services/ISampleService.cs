using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vp.Models;

namespace vp.Services
{
    public interface ISampleService
    {
        Task  AddSample(SampleModel sample);
        Task<List<SampleModel>> GetSamples();

    }
}

