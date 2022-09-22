using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.services;
using vp.models;
using vp.util;

namespace vp.Functions.Sample
{
    public class SampleUpload
    {
        private readonly ISampleService _sampleService;

        public SampleUpload(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [FunctionName("sample_upload")]
        public async Task<SampleModel> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {

            SampleModel sampleMetadata = JsonConvert.DeserializeObject<SampleModel>(req.Form["data"]);
            sampleMetadata.fileId = $"{Guid.NewGuid()}";
            await _sampleService.AddSample(sampleMetadata);

            var form = req.Form;
            string filename = $"{sampleMetadata._id}";

            var sample = form.Files["sample"];
            Utils.UploadFormFileAsync(sample, Config.SampleBlobContainerName, filename);

            var image = form.Files["image"];
            Utils.UploadFormFileAsync(image, Config.CoverArtContainerName, filename);

            return sampleMetadata;
        }
    }
}
