using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using vp.services;
using System.IO;
using Newtonsoft.Json;
using Azure.Storage.Sas;

namespace vp.functions.user
{
    public class GetPurchasedSamples
    {
        IStorageService _storageService;

        public GetPurchasedSamples(IStorageService storageService) {
            _storageService = storageService;
        }

        [FunctionName(FunctionNames.GetPurchasedSample)]
        public async Task<IActionResult> Run (
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string sampleName = JsonConvert.DeserializeObject<string>(requestBody);

            //TODO: Get the name of the zip... Or zips? :| Each purchase is per pack.. :D
            //TODO: WTF is this????
            var result = _storageService.GetSASURIForSampleHDBlob("63fa6f062a41786b1656d793.wav", BlobSasPermissions.Read);
            if(result == null)
            {
                return new BadRequestResult();
            }

            return new OkObjectResult(result.ToString());
        }

    }
}
