using System;
using System.Threading.Tasks;
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
        private readonly ILogger<SampleUpload> _log;

        public SampleUpload(ISampleService sampleService, ILogger<SampleUpload> log)
        {
            _sampleService = sampleService;
            _log = log;
        }

        [FunctionName("sample_upload")]
        public async Task<SampleModel> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {
            SampleModel sampleMetadata = new SampleModel();
            try
            {
                sampleMetadata = JsonConvert.DeserializeObject<SampleModel>(req.Form["data"]);
                await _sampleService.AddSample(sampleMetadata);
            }catch(Exception e)
            {
                log.LogError(e, "Failed to deserialize incoming sample json:");
            }

            
            

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
