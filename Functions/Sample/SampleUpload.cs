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
using vp.Models;

namespace vp.Functions.Sample
{
    public class SampleUpload
    {
        private readonly ISampleService _sampleService;
        private readonly IUserService _userService;
        private readonly ILogger<SampleUpload> _log;

        public SampleUpload(ISampleService sampleService, IUserService userService, ILogger<SampleUpload> log)
        {
            _sampleService = sampleService;
            _userService = userService;
            _log = log;
        }

        [FunctionName("sample_upload")]
        public async Task<SampleModel> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {
            SampleModel sampleMetadata = new SampleModel();
            sampleMetadata = JsonConvert.DeserializeObject<SampleModel>(req.Form["data"]);

            await _sampleService.AddSample(sampleMetadata);
            await _userService.AddForSale(req.Form["accountId"], sampleMetadata._id );
                 
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
