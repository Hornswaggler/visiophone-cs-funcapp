using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableFunctionVideoProcessor;

static class Utils
{
    public static bool IsInDemoMode => Environment.GetEnvironmentVariable("DemoMode") == "true";

    public static string GetTempTranscodeFolder()
    {
        var outputFolder = Path.Combine(Path.GetTempPath(), "transcodes", $"{DateTime.Today:yyyy-MM-dd}");
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
    }

    public static string GetReadSas(this BlobClient blob, TimeSpan validDuration)
    {
        var sas = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow + validDuration);
        return sas.ToString();
    }

    private static HttpClient client;
    public static async Task<string> DownloadToLocalFileAsync(string uri)
    {
        var extension = Path.GetExtension(new Uri(uri).LocalPath);
        var outputFilePath = Path.Combine(GetTempTranscodeFolder(), $"{Guid.NewGuid()}{extension}");
        client = client??new HttpClient();
        using (var downloadStream = await client.GetStreamAsync(uri))
        using (var s = File.OpenWrite(outputFilePath))
        {
            await downloadStream.CopyToAsync(s);
        }
        return outputFilePath;
    }

    public static async Task<string> TranscodeAndUpload(TranscodeParams transcodeParams, BlobClient outputBlob, ILogger log, ExecutionContext context)
    {
        var outputFilePath = Path.Combine(GetTempTranscodeFolder(), $"{Guid.NewGuid()}{transcodeParams.OutputExtension}");
        //try
        //{
        // TODO: Is the blob Name inside the outputBlob?
        await FfmpegWrapper.Transcode(transcodeParams.InputFile, transcodeParams.FfmpegParams, outputFilePath, log);
        //}
        //finally
        //{
        //    TryDeleteFiles(log, outputFilePath);
        //}

        return outputFilePath;
    }

    public static void TryDeleteFiles(ILogger log, params string[] files)
    {
        foreach (var file in files)
        {
            try
            {
                if (!String.IsNullOrEmpty(file) && File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                log.LogError($"Failed to clean up temporary file {file}", e);
            }
        }
    }

    public static string CreateLocalConcat(params string[] inputs)
    {
        var fileList = Path.Combine(GetTempTranscodeFolder(), $"{Guid.NewGuid()}.txt");
        File.WriteAllLines(fileList, inputs.Select(f => $"file '{f}'"));
        return fileList;
    }
}