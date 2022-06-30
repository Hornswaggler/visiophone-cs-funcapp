using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.Services;
using vp.Models;
using System.IO;
using System.Diagnostics;

namespace vp.Functions
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
            //string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            //Directory.CreateDirectory(tempPath);
            //log.LogError(new Exception("FUCK!"), $"Bad request: {tempPath}, Exists: {Directory.Exists(tempPath)}");


            var wavTemp = "b8d54e53-f51d-4109-a8b2-f9502e0e995e.wav";
            var tempPath = $"{context.FunctionAppDirectory}/ptemp";//Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempOut = Guid.NewGuid() + ".mp3";


            Directory.SetCurrentDirectory(context.FunctionAppDirectory + "/ptemp");


            var exePath = $"./ffmpeg.exe";

            //string exePath = @"D:\home\site\wwwroot\ffmpeg.exe";
            string wavTempFile = $"{wavTemp}";
            string mp3TempFile = $"{tempOut}";
            string arguments = $"-i {wavTempFile} -b:a 256k {mp3TempFile}";

            log.LogError(new Exception("Does the wav temp path exist?"), $"Exists: {File.Exists(wavTempFile)}");
            log.LogError(new Exception("Wav Temp"), $"{wavTempFile}");
            log.LogError(new Exception("mp3 Temp"), $"{mp3TempFile}");
            log.LogError(new Exception("args"), $"{arguments}");

            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            string eOut = null;
            p.StartInfo.RedirectStandardError = true;
            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            { eOut += e.Data; });
            p.StartInfo.FileName = exePath;
            p.StartInfo.Arguments = arguments;
            p.Start();

            // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
            string output = await p.StandardOutput.ReadToEndAsync();
            //p.BeginErrorReadLine();
            string error = await p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();

            log.LogError(new Exception("Process exited"), $"EO:{eOut}, E:{error}, O:{output}");


            return new StatusCodeResult(StatusCodes.Status201Created);

            //try
            //{
            //    var sample = req.Form.Files[0];
            //    SampleModel data = JsonConvert.DeserializeObject<SampleModel>(req.Form["data"]);

            //    await _sampleService.AddSample(data);


            //    //TODO: Update to accomodate multiple files per upload
            //    Guid guid = Guid.NewGuid();
            //    string fileName = $"{guid}_{sample.FileName}";
            //    var file = req.Form.Files[0];
            //    using (var stream = file.OpenReadStream())
            //    {
            //        string connectionString = Environment.GetEnvironmentVariable(Config.StorageConnectionString);
            //        string containerName = Environment.GetEnvironmentVariable(Config.SampleBlobContainerName);
            //        UploadManager uploadManager = new UploadManager(Config.StorageConnectionString);
            //        await uploadManager.UploadStreamAsync(stream, fileName);
            //    }

            //    //Convert / create MP3 version(s)

            //    return new StatusCodeResult(StatusCodes.Status201Created);

            //}
            //catch (Exception e)
            //{
            //    log.LogError(e, "Bad request");
            //    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            //}
        }
    }
}
