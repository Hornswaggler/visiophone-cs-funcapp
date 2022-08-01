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

namespace vp.functions
{
    public class upload_sample
    {
        private readonly ISampleService _sampleService;

        public upload_sample(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [FunctionName("upload_sample")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {
            try
            {
                var sample = req.Form.Files[0];
                SampleRequest data = JsonConvert.DeserializeObject<SampleRequest>(req.Form["data"]);

                await _sampleService.AddSample(data);

                //TODO: Update to accomodate multiple files per upload
                Guid guid = Guid.NewGuid();
                string fileName = $"{guid}_{sample.FileName}";
                var file = req.Form.Files[0];
                using (var stream = file.OpenReadStream())
                {
                    string connectionString = Environment.GetEnvironmentVariable(Config.StorageConnectionString);
                    string containerName = Environment.GetEnvironmentVariable(Config.SampleBlobContainerName);
                    await Utils.UploadStreamAsync(stream, fileName);
                }

                return new StatusCodeResult(StatusCodes.Status201Created);
            }
            catch (Exception e)
            {
                log.LogError(e, "Bad request");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
