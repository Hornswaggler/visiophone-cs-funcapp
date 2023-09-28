using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vp.functions;
using vp.services;
using vp.functions.stripe;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading;

namespace visiophone_cs_funcapp.Functions.SamplePack
{

    public class SamplePackTransactionStatus : AuthStripeBase
    {
        public SamplePackTransactionStatus(IUserService userService, IStripeService stripeService, IValidationService validationService)
            : base(userService, stripeService, validationService) { }

        [FunctionName(FunctionNames.SamplePackTransactionStatus)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            ILogger log)
        {
            StripeProfileResult account;
            try
            {
                account = await AuthorizeStripeUser(req);
            }
            catch (UnauthorizedAccessException e)
            {
                log.LogWarning("Unauthorized samplepack upload", e);
                return new UnauthorizedResult();
            }

            var status = await orchestrationClient.GetStatusAsync("710dad9eca41427397126b9af7f854fb");

            //var results = await orchestrationClient.ListInstancesAsync(
            //    new OrchestrationStatusQueryCondition
            //    {
            //        RuntimeStatus = new[]
            //        {
            //            OrchestrationRuntimeStatus.Pending,
            //            OrchestrationRuntimeStatus.Running
            //        },
            //    },
            //    CancellationToken.None
            //);


            return new OkObjectResult("yes");
        }
    }
}
