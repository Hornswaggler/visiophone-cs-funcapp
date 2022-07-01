﻿using System;
using System.IO;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
//using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using visiophone_cs_funcapp.Orchestrations;

namespace visiophone_cs_funcapp.Functions;

public static class ProcessVideoFunctions
{
    //[FunctionName(nameof(ProcessVideoStarter))]
    //public static async Task<IActionResult> ProcessVideoStarter(
    //    [HttpTrigger(AuthorizationLevel.Function, "get", "post",
    //        Route = null)] HttpRequest req,
    //    [DurableClient] IDurableOrchestrationClient starter,
    //    ILogger log)
    //{
    //    // parse query parameter
    //    string video = req.GetQueryParameterDictionary()["video"];

    //    if (video == null)
    //        return new BadRequestObjectResult("Please pass a video location on the query string");

    //    log.LogInformation($"Attempting to start video processing for {video}.");
    //    var instanceId = await starter.StartNewAsync(OrchestratorNames.ProcessAudio, video);
    //    return starter.CreateCheckStatusResponse(req, instanceId);
    //}

    //[FunctionName(nameof(StartPeriodicTask))]
    //public static async Task<IActionResult> StartPeriodicTask(
    //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
    //    HttpRequest req,
    //    [DurableClient] IDurableOrchestrationClient client,
    //    ILogger log)
    //{
    //    var instanceId = "PeriodicTask";
    //    await client.StartNewAsync(OrchestratorNames.PeriodicTask, instanceId, 0);
    //    return client.CreateCheckStatusResponse(req, instanceId);
    //}

    [FunctionName(nameof(AutoProcessUploadedVideos))]
    public static async Task AutoProcessUploadedVideos(
        [BlobTrigger("uploads/{name}", Connection = "STORAGE_CONNECTION_STRING")] Stream myBlob, string name,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        try
        {

            string id = new Guid().ToString();
            var orchestrationId = await starter.StartNewAsync<string>(
                OrchestratorNames.ProcessAudio, name);

            log.LogInformation($"Started an orchestration {orchestrationId} for uploaded video {name}");
        }
        catch (Exception e)
        {
            log.LogError(new Exception("Failed:", e), "Start Orchestration Failed");
        }
    }
}
