using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Lame;
using Azure.Storage.Blobs.Specialized;
using visiophone_cs_funcapp;

namespace vp
{
    public class DSample
    {
        [FunctionName("DSample")]
        public void Run([BlobTrigger("samples/{name}", Connection= "STORAGE_CONNECTION_STRING")]Stream myBlob, string name, ILogger log)
        {
            int extensionLocation = name.LastIndexOf('.');
            if (extensionLocation != -1 && name.Substring(extensionLocation + 1).ToLower() == "wav")
            {
                WaveToMP3(name, $"{name}.mp3", myBlob);
            }
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }

        public async static void WaveToMP3(string waveFileName, string mp3FileName, Stream astream, int bitRate = 128)
        {
            using (var retMs = new MemoryStream())
            using (var ms = new MemoryStream())
            using (var rdr = new WaveFileReader(astream))
            
            {
                BlockBlobClient blobClient = BlockBlobClientFactory.MakeSampleBlockBlobClient(waveFileName);
                await blobClient.DownloadToAsync(ms);
                var pcmMs = new MemoryStream();
                var pcmWriter = new WaveFileWriter(pcmMs, new WaveFormat(44100, 2));

                using (var converter = WaveFormatConversionStream.CreatePcmStream(rdr))
                using (var downSampler = new WaveFormatConversionStream(new WaveFormat(44100, 16, 2), converter)) {
                    downSampler.CopyTo(pcmMs);
                    pcmMs.Position = 0;
                }

                ms.Position = 0;
                // TODO: rdr.WaveFormat dictates src encoding bit depth
                using (var pcmRdr = new WaveFileReader(pcmMs))
                using (var fs = new LameMP3FileWriter(retMs, rdr.WaveFormat, Config.PreviewBitrate)) {
                    pcmRdr.CopyTo(fs);
                    fs.Flush();

                    retMs.Position = 0;
                    UploadManager uploadManager = new UploadManager(Config.StorageConnectionString);
                    await uploadManager.UploadStreamAsync(retMs, mp3FileName);
                }
            }
        }

        public static void MP3ToWave(string mp3FileName, string waveFileName)
        {
            using (var reader = new Mp3FileReader(mp3FileName))
            using (var writer = new WaveFileWriter(waveFileName, reader.WaveFormat))
                reader.CopyTo(writer);
        }
    }
}
