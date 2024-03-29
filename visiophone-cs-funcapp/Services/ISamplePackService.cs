﻿using System.Collections.Generic;
using System.Threading.Tasks;
using vp.models;
using vp.functions.samplepack;

namespace vp.services
{
    public interface ISamplePackService
    {
        Task<SamplePack<Sample>> AddSamplePack(SamplePack<Sample> samplePack);
        Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksByName(SearchQueryRequest request);
        Task<SamplePack<Sample>> GetSamplePackById(string samplePackId);
        Task<SearchQueryResult<SamplePack<Sample>>> GetSamplePacksBySellerId(SearchQueryRequest request);
        Task<List<SamplePack<Sample>>> GetSamplePackPurchasesByPriceIds(List<string> priceIds);
        Task<SamplePack<Sample>> DeleteSamplePack(SamplePack<Sample> samplePack);
    }
}
