using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace vp
{
    class FfmpegAudioProcessor : IAudioProcessor
    {
        public async Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log)
        {
            return await Utils.Transcode(transcodeParams, log);
        }

        public Task PublishAudio(string tempFileLocation, BlockBlobClient blobClient)
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
                        blockIds.Add(blockId);
                        counter++;
                    }
                } while (bytesRemaining > 0);

                // TODO: should come from request
                var headers = new BlobHttpHeaders()
                {
                    ContentType = "audio/wav"
                };
                blobClient.CommitBlockList(blockIds, headers);

                return Task.FromResult<object>(null);
            }
        }

        //public Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<string> TranscodeAsync(TranscodeParams transcodeParams, ILogger log, IDurableOrchestrationContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}

