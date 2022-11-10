using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.util;
using vp;
using System.Security.Claims;
using System.Linq;
using vp.DTO;


namespace visiophone_cs_funcapp.Functions.UserLibrary
{
    public static class GetLibraryItem
    {
        [FunctionName("get_library_item")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ClaimsPrincipal principal,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            LibraryItemRequest request = JsonConvert.DeserializeObject<LibraryItemRequest>(requestBody);

            var sasUri = BlobFactory.GetBlobSasToken(Config.UserLibraryContainerName, "WAFERLOGIC.mp3");

            //var jsonString = JsonConvert.SerializeObject(
            //principal, Formatting.Indented,
            //new JsonConverter[] { new StringEnumConverter() });


            //BlobSasBuilder sasBuilder = new BlobSasBuilder()
            //{
            //    BlobContainerName = containerName,
            //    Resource = "c"
            //};

            //sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
            //sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            //Uri sasUri = container.GenerateSasUri(sasBuilder);
            //Utils.GetReadSas()

            var result = principal.Identities.Where(identity => identity.Claims.Any(claim => claim.Value == "2d1e671b-65ba-40be-b119-5cb56ca78e80"));
            bool IsAuthenticated = principal.Identity.IsAuthenticated;

            log.LogInformation($"Found: {result.Count()}, Authenticated: {IsAuthenticated}");

            foreach (var identity in principal.Identities)
            {
                log.LogInformation($"{identity.AuthenticationType}");
                foreach(var claim in identity.Claims)
                {
                    log.LogInformation($"Issuer: {claim.Issuer}, Value: {claim.Value}");
                }
            }

            return new OkObjectResult(":)");
        }
    }
}
