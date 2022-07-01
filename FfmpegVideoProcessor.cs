using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableFunctionVideoProcessor;

class FfmpegVideoProcessor : IVideoProcessor
{
    public async Task<string> TranscodeAsync(TranscodeParams transcodeParams, BlobClient outputBlob, ILogger log, ExecutionContext context)
    {
        return await Utils.TranscodeAndUpload(transcodeParams, outputBlob, log, context);
    }

    public async Task<string> PrependIntroAsync(
        BlobClient outputBlob,
        string introLocation, 
        string incomingFile,
        ILogger log)
    {
        var localIntro = "";
        var localIncoming = "";
        var localConcat = "";

        try
        {
            localIntro = await Utils.DownloadToLocalFileAsync(introLocation);
            localIncoming = await Utils.DownloadToLocalFileAsync(incomingFile);
            localConcat = Utils.CreateLocalConcat(localIntro, localIncoming);
            var transcodeParams = new TranscodeParams
            {
                OutputExtension = ".mp4",
                InputFile = incomingFile,
                FfmpegParams = $"-f concat -safe 0 -i \"{localConcat}\" -codec copy "
            };
            return await Utils.TranscodeAndUpload(transcodeParams, outputBlob, log, null);
        }
        finally
        {
            Utils.TryDeleteFiles(log, localIntro, localIncoming, localConcat);
        }
    }

    public async Task<string> ExtractThumbnailAsync(string incomingFile, BlobClient outputBlob, ILogger log)
    {
        
        var transcodeParams = new TranscodeParams
        {
            OutputExtension = ".png",
            InputFile = incomingFile,
            FfmpegParams = "-vf  \"thumbnail,scale=640:360\" -frames:v 1"
        };
        return await Utils.TranscodeAndUpload(transcodeParams, outputBlob, log, null);
    }

    public Task PublishVideo(string tempFileLocation, BlockBlobClient blobClient)
    {

        using (FileStream fs = new FileStream(tempFileLocation, FileMode.Open))
        {
            int offset = 0;
            int counter = 0;
            List<string> blockIds = new List<string>();

            var bytesRemaining = fs.Length;
            do
            {
                var dataToRead = Math.Min(bytesRemaining, 1 * 1024 * 1024);
                byte[] data = new byte[dataToRead];
                var dataRead = fs.Read(data, offset, (int)dataToRead);
                bytesRemaining -= dataRead;
                if (dataRead > 0)
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(counter.ToString("d6")));
                    blobClient.StageBlock(blockId, new MemoryStream(data));
                    Console.WriteLine(string.Format("Block {0} uploaded successfully.", counter.ToString("d6")));
                    blockIds.Add(blockId);
                    counter++;
                }
            } while (bytesRemaining > 0);

            // TODO should come from request
            var headers = new BlobHttpHeaders()
            {
                ContentType = "audio/wav"
            };
            blobClient.CommitBlockList(blockIds, headers);

            return Task.FromResult<object>(null);

            return Task.Delay(5000);

        }
    }

    public Task RejectVideo(string[] videoLocations)
    {
        // TODO: move files to rejected location
        return Task.Delay(5000);
    }
}