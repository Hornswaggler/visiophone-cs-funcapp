using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using NAudio.Lame;
using NAudio.Wave;
using vp.models;
using vp.orchestrations.processaudio;

namespace vp.orchestrations.upsertsample
{

    //THIS WONT WORK

//    var mediaServicesResourceId = MediaServicesAccountResource.CreateResourceIdentifier(
//        subscriptionId: "e1896010-0921-499e-a947-f5aef5306277",
//        resourceGroupName: "visophone-east-us2",
//        accountName: "visiophonemediaservices"
//    );

//    var credential = new DefaultAzureCredential();
//    var armClient = new ArmClient(credential);

//    var mediaServicesAccount = armClient.GetMediaServicesAccountResource(mediaServicesResourceId);

//    CreateTransformAsync(mediaServicesAccount, "");


//    //// In this example, we are assuming that the Asset name is unique.
//    MediaAssetResource asset;

//    string assetName = "sounds.wav";
//            try
//            {
//                asset = await mediaServicesAccount.GetMediaAssets().GetAsync(assetName);

//    // The Asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
//    // an existing Asset, use an unique name.
//    Console.WriteLine($"Warning: The Asset named {assetName} already exists. It will be overwritten.");
//            }
//            catch (RequestFailedException)
//{
//    // Call Media Services API to create an Asset.
//    // This method creates a container in storage for the Asset.
//    // The files (blobs) associated with the Asset will be stored in this container.
//    Console.WriteLine("Creating an input Asset...");
//    asset = (await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(WaitUntil.Completed, assetName, new MediaAssetData())).Value;
//}

//// Use Media Services API to get back a response that contains
//// SAS URL for the Asset container into which to upload blobs.
//// That is where you would specify read-write permissions 
//// and the expiration time for the SAS URL.
//var sasUriCollection = asset.GetStorageContainerUrisAsync(
//    new MediaAssetStorageContainerSasContent
//    {
//        Permissions = MediaAssetContainerPermission.ReadWrite,
//        ExpireOn = DateTime.UtcNow.AddHours(1)
//    }).GetAsyncEnumerator();

//await sasUriCollection.MoveNextAsync();
//var sasUri = sasUriCollection.Current;

////// Use Storage API to get a reference to the Asset container
////// that was created by calling Asset's CreateOrUpdate method.
//var container = new BlobContainerClient(sasUri);
//BlobClient blob = container.GetBlobClient("sounds.wav");

//using WebClient client = new WebClient();
//client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

//using Stream stream = client.OpenRead("https://www.visiophone.wtf/transcoded/flyfartsonice.wav");

////// Use Storage API to upload the file into the container in storage.
////Console.WriteLine("Uploading a media file to the Asset...");
//await blob.UploadAsync(stream);

//stream.Close();




public class UpsertSampleOrchestrator
    {
        [FunctionName(OrchestratorNames.UpsertSample)]
        public static async Task<Sample> UpsertSample(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
           // BlobServiceClient _blobServiceClient = new BlobServiceClient(Config.StorageConnectionString);
           // BlobContainerClient sampleContainer = _blobServiceClient.GetBlobContainerClient(Config.SampleFilesContainerName);

           // //var blobClient = sampleContainer.GetBlobClient("flyfartsonice.wav"); // GetBlockBlobClient($"{processAudioTransaction.sampleId}.{processAudioTransaction.fileExtension}");
           // var outputClient = sampleContainer.GetBlobClient("flyfartsoniceout.wav"); // GetBlockBlobClient($"{processAudioTransaction.sampleId}.{processAudioTransaction.fileExtension}");

           // //BlockBlobClient outputClient = BlobFactory.MakeSampleBlockBlobClient(name, containerName);
           // //BlockBlobClient outputClient = BlobFactory.MakeSampleBlockBlobClient(name, containerName);

           // //return container.GetBlockBlobClient(blobName);

           // //string sq = /* URL of WAV file (http://foo.com/blah.wav) */

           // //Response.ContentType = "audio/mpeg";


           // //using WebClient client = new WebClient();
           // //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

           // //using Stream stream = client.OpenRead("https://www.visiophone.wtf/transcoded/flyfartsonice.wav");

            
           // var outputStream = outputClient.OpenWrite(overwrite: true, options: new BlobOpenWriteOptions());
           // //Stream outputStream = await outputClient.OpenWriteAsync(true);

           // using (HttpClient client = new HttpClient())
           // {
           //     var wavFormat = new WaveFormat(48000, 2);

           //     using (var stream = await client.GetStreamAsync("https://www.visiophone.wtf/transcoded/16bit4800khzffoi.wav"))
           //     {



           //         //using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(new
           //         //    Mp3FileReader(stream)))
           //         //using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
           //         //{
           //         //    byte[] bytes = new byte[waveStream.Length];
           //         //    waveStream.Position = 0;
           //         //    waveStream.Read(bytes, 0, (int) waveStream.Length);
           //         //    waveFileWriter.WriteData(bytes, 0, bytes.Length);
           //         //    waveFileWriter.Flush();
           //         //}


           //         //using (var reader = new MediaFoundationReader(infile))


           //         using (WaveStream waveStream = new RawSourceWaveStream(stream, wavFormat))
           //         {

           //             using (var wavWriter = new LameMP3FileWriter(outputStream, wavFormat, LAMEPreset.ABR_128))
           //             {
           //                 //try
           //                 //{
           //                 //    waveStream.CopyTo(wavWriter);
           //                 //    waveStream.
           //                 //}
           //                 //catch (Exception e)
           //                 //{
           //                 //    var i = 0;
           //                 //    i++;
           //                 //}
           //             }
           //         }
                    
           // }



           // /**

           //    //public static Sound Read(Stream source)
           //    //{
           //    //BinaryReader reader = new BinaryReader(stream);
           //    ////Sound result = new Sound();

           //    //int id = reader.ReadInt32();
           //    //if (id == 0x00394453)
           //    //{
           //    //    int headerLength = reader.ReadInt32();
           //    //    int sampleLength = reader.ReadInt32();

           //    //    headerLength -= 12;
           //    //    if (headerLength > 0)
           //    //        reader.ReadBytes(headerLength);

           //    //byte[] wavData = reader.ReadBytes(sampleLength);
           //    //using (MemoryStream wavDataMem = new MemoryStream(stream))
           //    //{
           //    //    //using (WaveStream wavStream = new WaveFileReader(wavDataMem))
           //    //    using (WaveStream waveStream = new RawSourceWaveStream(sound1, wavin.WaveFormat)(wavDataMem))
           //    //    {
           //    //        byte[] rawWaveData = new byte[wavStream.Length];
           //    //        wavStream.Read(rawWaveData, 0, (int)wavStream.Length);



           //    //        //result.SetSound(rawWaveData, wavStream.WaveFormat);
           //    //    }
           //    //}
           //        //}

           //    //    return result;
           //    //}




















           //    //try
           //    //{
           //    //    //using (MemoryStream wavDataMem = new MemoryStream(wavData))

           //    //    using (var wavReader = new WaveFileReader(stream))
           //    //    {
           //    //        var i = 0;
           //    //        i++;
           //    //    }
           //    //}
           //    //catch (Exception e)
           //    //{
           //    //    var i = 0;
           //    //    i++;
           //    //}

           //    //    {
           //    //        using (var wavWriter = new LameMP3FileWriter(outputStream, wavReader.WaveFormat, LAMEPreset.ABR_128))
           //    //        {

           //        //        }
           //        //    }
           //        //}
           //        //catch (Exception ex)
           //        //{
           //        //    int i = 0;
           //        //    i++;
           //        //}
           ////}

           //**/


            //OLD CODE... :|
            UpsertSampleTransaction transaction = ctx.GetInput<UpsertSampleTransaction>();
            ProcessAudioTransaction audioTransaction = new ProcessAudioTransaction
            {
                fileExtension = transaction.request.fileExtension,
                sampleId = transaction.request.id,
                samplePackId = transaction.samplePackId,
                incomingFileName = transaction.request.clipUri
            };

            var processAudioResult = await ctx.CallSubOrchestratorAsync<ProcessAudioTransaction>(
                OrchestratorNames.ProcessAudio,
                audioTransaction
            );



            var sample = SampleFactory.MakeSampleForSampleRequest(transaction.request);

            return sample;
        }

        private static async Task<MediaTransformResource> CreateTransformAsync(MediaServicesAccountResource mediaServicesAccount, string transformName)
        {
            // Create the custom Transform with the outputs defined above
            // Does a Transform already exist with the desired name? This method will just overwrite (Update) the Transform if it exists already. 
            // In production code, you may want to be cautious about that. It really depends on your scenario.
            var transform = await mediaServicesAccount.GetMediaTransforms().CreateOrUpdateAsync(
                WaitUntil.Completed,
                transformName,
                new MediaTransformData
                {
                    Outputs =
                    {
                // Create a new TransformOutput with a custom Standard Encoder Preset using the HEVC (H265Layer) codec
                // This demonstrates how to create custom codec and layer output settings
                new MediaTransformOutput(
                    preset: new StandardEncoderPreset(
                        codecs: new MediaCodecBase[]
                        {
                            // Add an AAC Audio layer for the audio encoding
                            new AacAudio
                            {
                                Channels = 2,
                                SamplingRate = 48000,
                                Bitrate = 128000,
                                Profile = AacAudioProfile.AacLc
                            },
                            // Next, add a H264Video for the video encoding
                            new H264Video
                            {
                                // Set the GOP interval to 2 seconds for all H264Layers
                                KeyFrameInterval = TimeSpan.FromSeconds(2),
                        
                                // Add H264Layers, one at HD and the other at SD. Assign a label that you can use for the output filename.
                                Layers =
                                {
                                    new H264Layer(bitrate: 1000000)
                                    {
                                        Width = "1280",
                                        Height = "720",
                                        Label = "HD"
                                    },
                                    new H264Layer(bitrate: 600000)
                                    {
                                        Width = "640",
                                        Height = "360",
                                        Label = "SD"
                                    }
                                }
                            },
                            // Also generate a set of thumbnails in one JPG file (thumbnail sprite)
                            new JpgImage(start: "25%")
                            {
                                Start = "0%",
                                Step = "5%",
                                Range = "100%",
                                SpriteColumn = 10,
                                Layers =
                                {
                                    new JpgLayer
                                    {
                                        Width = "20%",
                                        Height = "20%",
                                        Quality = 90
                                    }
                                }
                            }
                        },
                        // Specify the format for the output files - one for video+audio, and another for the thumbnails
                        formats: new MediaFormatBase[]
                        {
                            // Mux the H.264 video and AAC audio into MP4 files, using basename, label, bitrate and extension macros
                            // Note that since you have multiple H264Layers defined above, you have to use a macro that produces unique names per H264Layer
                            // Either {Label} or {Bitrate} should suffice
                            new Mp4Format(filenamePattern: "Video-{Basename}-{Label}-{Bitrate}{Extension}"),
                            new JpgFormat(filenamePattern: "Thumbnail-{Basename}-{Index}{Extension}")
                        }
                    )
                )
                {
                    OnError = MediaTransformOnErrorType.StopProcessingJob,
                    RelativePriority = MediaJobPriority.Normal
                }
                    },
                    Description = "A simple custom encoding transform with 2 MP4 bitrates and thumbnail sprite"
                });

            return transform.Value;
        }

        internal class SeekableBlobReadStream : Stream
        {
            private Stream stream;
            private readonly BlobBaseClient client;
            private readonly BlobProperties properties;

            public SeekableBlobReadStream(BlobBaseClient client)
            {
                properties = client.GetProperties().Value;
                this.client = client;
            }

            private void PrepareStream(long position = 0)
            {
                if (stream == null || position != 0)
                {
                    if (stream != null)
                        stream.Dispose();
                    stream = client.OpenRead(new BlobOpenReadOptions(false) { Position = position });
                }
            }

            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite => false;

            public override long Length => properties.ContentLength;

            public override long Position
            {
                get => (stream?.Position).GetValueOrDefault();
                set => PrepareStream(value);
            }

            public override void Flush()
            {
                PrepareStream();
                stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                PrepareStream();
                return stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Current:
                        offset = Position + offset;
                        break;
                    case SeekOrigin.End:
                        offset = Length - offset;
                        break;
                }
                PrepareStream(offset);
                return stream.Position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }

    }
}
