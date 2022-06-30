using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Azure.Storage.Blobs.Specialized;
using visiophone_cs_funcapp;
using System;
using System.Diagnostics;
using Azure.Storage.Blobs;

namespace vp
{
    public class DSample
    {
        //[FunctionName("DSample")]
        //public void Run([BlobTrigger("samples/{name}", Connection = "STORAGE_CONNECTION_STRING")] Stream myBlob, string name, ILogger log, ExecutionContext context)
        //{
        //    log.LogError(new Exception("Blob trigger fired!"), "Getting Extension");
        //    int extensionLocation = name.LastIndexOf('.');
        //    if (extensionLocation != -1 && name.Substring(extensionLocation + 1).ToLower() == "wav")
        //    {
        //        try
        //        {
        //            WaveToMP3(name, $"{name}.mp3", myBlob, log, context);
        //        }
        //        catch (Exception e)
        //        {
        //            log.LogError(e, "Failed to process sample");
        //        }
        //    }
        //    log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        //}

        public async static void WaveToMP3(string waveFileName, string mp3FileName, Stream astream, ILogger log, ExecutionContext context, int bitRate = 128)
        {
            log.LogError(new Exception("Inside Wav to MP3!"), "Getting Extension");

            using (var retMs = new MemoryStream())
            using (var ms = new MemoryStream())
            using (var rdr = new WaveFileReader(astream))
            {
                log.LogError(new Exception("Streams Opened"), "Getting Pathings");

                var fred = $"{Environment.CurrentDirectory}\\ffmpeg.exe";

                log.LogError(new Exception("Current Dir:"), fred);



                var temp = Path.GetTempFileName();
                var tempOut = Guid.NewGuid() + ".mp3";

                var tempPath = $"{context.FunctionAppDirectory}\\ptemp";//Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                Directory.CreateDirectory(tempPath);

                var wavTemp = Guid.NewGuid() + ".wav";

                log.LogError(new Exception("Wav Path"), $"{tempPath}\\{wavTemp}");

                log.LogError(new Exception($"Context: {context.FunctionAppDirectory}"), "asdf");

                using (var fs = new FileStream($"{tempPath}\\{wavTemp}", FileMode.Create))
                {
                    log.LogError(new Exception("Downloading Block"), Config.StorageConnectionString);

                    //try
                    //{
                    //BlockBlobClient blobClient = BlockBlobClientFactory.MakeSampleBlockBlobClient(waveFileName);

                    BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
                    log.LogError(new Exception("Got Client"), Config.StorageConnectionString);

                    BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(Config.SampleBlobContainerName);
                    log.LogError(new Exception("Got Container"), Config.StorageConnectionString);

                    BlockBlobClient blobClient = container.GetBlockBlobClient(waveFileName);





                    log.LogError(new Exception("Succesfully Made the block blob client"), "Good");
                    var resp = await blobClient.DownloadToAsync(fs);
                    log.LogError(new Exception("Completed Downloading Blob"), resp.ToString());

                    fs.Dispose();
                    //}
                    //catch (Exception e) {
                    //    log.LogError(new Exception("That Is Shitty.....", e), "MakeBlockBlobClientFailed");

                    //}

                }


                var exePath = $"{context.FunctionAppDirectory}\\ffmpeg.exe";

                //string exePath = @"D:\home\site\wwwroot\ffmpeg.exe";
                string wavTempFile = $"\"{tempPath}\\{wavTemp}\"";
                string mp3TempFile = $"\"{tempPath}\\{tempOut}\"";
                string arguments = $"-i {wavTempFile} -b:a 256k {mp3TempFile}";
                log.LogError(new Exception("Does the wav temp path exist?"), $"Exists: {File.Exists(wavTempFile)}");
                log.LogError(new Exception("Wav Temp"), $"{wavTempFile}");
                log.LogError(new Exception("mp3 Temp"), $"{mp3TempFile}");
                log.LogError(new Exception("args"), $"{arguments}");


                //ProcessStartInfo psi = new ProcessStartInfo();
                //psi.UseShellExecute = false;

                //psi.FileName = exePath;
                //psi.Arguments = arguments;
                //psi.RedirectStandardOutput = true;
                //psi.RedirectStandardError = true;

                log.LogError(new Exception("Begining external process"), "adsfasdfasdf");




                Directory.SetCurrentDirectory(context.FunctionAppDirectory);

                var p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                string eOut = null;
                p.StartInfo.RedirectStandardError = true;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                { eOut += e.Data; });
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = arguments;
                p.Start();

                // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                //p.BeginOutputReadLine();
                string output = await p.StandardOutput.ReadToEndAsync();
                //p.BeginErrorReadLine();
                string error = await p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();

                log.LogError(new Exception("Process exited"), $"{error}, {output}");

                log.LogInformation($"The nMap Exe Result are:\n'{output}'");
                log.LogInformation($"\nError stream: {eOut}");
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

                //var process = Process.Start(psi);
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();
                //string output = process.StandardOutput.ReadToEnd();
                //await process.WaitForExitAsync();

                try
                {

                    using (var mp3fs = new FileStream($"{tempPath}\\{tempOut}", FileMode.Open))
                    {
                        log.LogError(new Exception("Opened the MP3 file"), "Ugh");
                        UploadManager uploadManager = new UploadManager(Config.StorageConnectionString);
                        await uploadManager.UploadStreamAsync(mp3fs, mp3FileName);
                        log.LogError(new Exception("Uploaded File!"), "Ugh");

                    }
                }
                catch (Exception e)
                {
                    log.LogError(new Exception("Failed to upload blob:", e), "Boom!");
                }

                // TODO: rdr.WaveFormat dictates src encoding bit depth
                //using (var pcmRdr = new WaveFileReader(pcmMs))
                //using (var fs = new LameMP3FileWriter(retMs, rdr.WaveFormat, Config.PreviewBitrate))
                //{
                //pcmRdr.CopyTo(fs);
                //fs.Flush();

                //retMs.Position = 0;

                //}



                //}




                //return new StatusCodeResult(StatusCodes.Status201Created);


                //var pcmMs = new MemoryStream();
                //var pcmWriter = new WaveFileWriter(pcmMs, new WaveFormat(44100, 2));

                //using (var converter = WaveFormatConversionStream.CreatePcmStream(rdr))
                //using (var downSampler = new WaveFormatConversionStream(new WaveFormat(44100, 16, 2), converter)) {
                //    downSampler.CopyTo(pcmMs);
                //    pcmMs.Position = 0;
                //}


            }
        }

        //public static void MP3ToWave(string mp3FileName, string waveFileName)
        //{
        //    using (var reader = new Mp3FileReader(mp3FileName))
        //    using (var writer = new WaveFileWriter(waveFileName, reader.WaveFormat))
        //        reader.CopyTo(writer);
        //}
    }
}
