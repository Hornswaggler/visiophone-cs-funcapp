using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace vp.util {
    static class Utils
    {
        public static string GetFileNameForId(string id, string incomingFileName) {
            return $"{id}.{GetExtensionForFileName(incomingFileName)}";
        }

        public static string GetExtensionForFileName(string filename) {
            string result = "";
            try
            {
                var parts = filename.Split('.');
                result = parts[parts.Length - 1];
            } catch
            {
                //consume
            }
            return result;

        }

        public static string GetReadSas(this BlobClient blob, TimeSpan validDuration)
        {
            var sas = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow + validDuration);
            return sas.ToString();
        }

        public static string GetFileExtension(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.'));
        }

        public static async Task<bool> CleanupTaskHub(
            IDurableOrchestrationClient client,
            ILogger log
        ) {
            await TerminateAllInstances(client, log);
            await Utils.PurgeWebJobHistory(client, DateTime.Now.Subtract(TimeSpan.FromDays(365)), DateTime.Now, log);

            return true;
        }

        public static async Task<bool> TerminateAllInstances(
            IDurableOrchestrationClient client,
            ILogger log
        ) {
            var instances = await GetAllInstances(client);
            foreach (var instance in instances)
            {
                try
                {
                    await TerminateInstance(client, instance);
                    log.LogInformation($"Deleted Instance: {JsonConvert.SerializeObject(instance)}");
                }
                catch (Exception e)
                {
                    log.LogInformation($"Delete Instance Failed: {e.Message}");

                }
            }
            return true;
        }

        public static async Task<List<DurableOrchestrationStatus>> GetAllInstances(
            IDurableOrchestrationClient client
        ) {
            var noFilter = new OrchestrationStatusQueryCondition();
            OrchestrationStatusQueryResult instances = await client.ListInstancesAsync(
                noFilter,
                CancellationToken.None);

            List<DurableOrchestrationStatus> result = new List<DurableOrchestrationStatus>();
            foreach (DurableOrchestrationStatus instance in instances.DurableOrchestrationState)
            {
                result.Add(instance);
            }
            return result;
        }

        public static async Task<bool> TerminateInstance(
            IDurableOrchestrationClient client,
            DurableOrchestrationStatus instance
        ) {
            string reason = "Found a bug";
            await client.TerminateAsync(instance.InstanceId, reason);
            return true;
        }


        public static async Task<bool> PurgeWebJobHistory(
            IDurableOrchestrationClient starter,
            DateTime from,
            DateTime to,
            ILogger log)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var instances = await starter.ListInstancesAsync(
                new OrchestrationStatusQueryCondition
                {
                    CreatedTimeFrom = from,
                    CreatedTimeTo = to,
                },
                token
            );

            foreach (var eachInstance in instances.DurableOrchestrationState)
            {
                try
                {
                    await starter.PurgeInstanceHistoryAsync(eachInstance.InstanceId);
                }
                catch (Exception e)
                {
                    log.LogError($"Failed to purge instance {eachInstance.InstanceId}, {e.Message}", e);
                    return false;
                }
            }

            return true;
        }
    }
}

