using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using vp;

namespace vp.util
{
    public class UploadManager
    {
        public Task UploadStreamAsync(Stream stream, string name)
        {
            BlockBlobClient blobClient = BlockBlobClientFactory.MakeSampleBlockBlobClient(name, Config.SampleBlobContainerName);
            int offset = 0;
            int counter = 0;
            List<string> blockIds = new List<string>();

            var bytesRemaining = stream.Length;
            do
            {
                var dataToRead = Math.Min(bytesRemaining, Config.BufferSize);
                byte[] data = new byte[dataToRead];
                var dataRead = stream.Read(data, offset, (int)dataToRead);
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
        }
    }
}
