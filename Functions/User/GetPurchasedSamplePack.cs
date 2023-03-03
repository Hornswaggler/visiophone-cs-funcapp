using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using vp.services;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace vp.functions.user
{
    public class GetPurchasedSamplePack
    {
        IStorageService _storageService;
        ISamplePackService _samplePackService;

        public GetPurchasedSamplePack(IStorageService storageService, ISamplePackService samplePackService)
        {
            _storageService = storageService;
            _samplePackService = samplePackService;
        }

        [FunctionName(FunctionNames.GetPurchasedSamplePack)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string samplePackId = JsonConvert.DeserializeObject<string>(requestBody);

            var samplePack = await _samplePackService.GetSamplePackById(samplePackId);
            var sampleLinks = new List<KeyValuePair<string, Uri>>();

            foreach(var sample in samplePack.samples)
            {
                sampleLinks.Add(
                    new KeyValuePair<string, Uri>(
                        sample._id,
                        _storageService.GetSASTokenForSampleBlob($"{sample._id}.wav")
                    )
                );
            }

            return new OkObjectResult(JsonConvert.SerializeObject(sampleLinks));
        }

    }
}
