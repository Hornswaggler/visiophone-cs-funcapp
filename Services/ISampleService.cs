using System;
using System.Threading.Tasks;
using vp.Models;

namespace vp.Services
{
    public interface ISampleService
    {
        Task  AddSample(Sample sample);
    }
}

