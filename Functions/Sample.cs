using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

// namespace vp
// {
//     public class Sample
//     {
//         private readonly ISampleService _sampleService;

//         public Sample(ISampleService sampleService)
//         {
//             _sampleService = sampleService;
//         }

//         [FunctionName("sample")]
//         public async Task<IActionResult> Run(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
//             ILogger log)
//         {
//             log.LogInformation("C# HTTP trigger function processed a request.");

//             string name = req.Query["name"];

//             string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//             dynamic data = JsonConvert.DeserializeObject(requestBody);
//             name = name ?? data?.name;

//             string responseMessage = string.IsNullOrEmpty(name)
//                 ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
//                 : $"Hello, {name}. This HTTP triggered function executed successfully.";

//             var result = await this._sampleService.GetSamples();

//             return new OkObjectResult(result);
//         }
//     }
// }
