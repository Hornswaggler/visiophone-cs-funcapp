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
        Task<SampleQueryResult> GetSamples(SampleRequest request);
        Task<Sample> GetSampleById(string id);
        Task<List<Sample>> GetSamplesById(IEnumerable<string> sampleIds);
        Task<Purchase> AddPurchase(Purchase purchase);
        Task<List<Purchase>> GetPurchases(string accountId);
        Task<List<Sample>> GetSamples(List<string> priceIds);

    }
}
