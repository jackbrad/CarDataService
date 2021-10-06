using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MediaServices.Client;
using System.IO;
using System.Threading;


namespace UploadVidExample
{
    class Program
    {

        static CloudMediaContext context;

        static void Main(string[] args)
        {
             // Set the project properties to use the full .NET Framework (not Client Profile)
            // With NuGet Package Manager, install windowsazure.mediaservices
            // add: using Microsoft.WindowsAzure.MediaServices.Client;
            var uploadFilePath = @"c:\Vehicle_5_Lap_27_473867038.mp4";
            context = new CloudMediaContext("nsinfractvid", "sy4kEiMGkk4WK8w7UPlffbUEv6cSlmAVGGq0Ld4w628=");
            var uploadAsset = context.Assets.Create(Path.GetFileNameWithoutExtension(uploadFilePath), AssetCreationOptions.None);
            var assetFile = uploadAsset.AssetFiles.Create(Path.GetFileName(uploadFilePath));
            assetFile.Upload(uploadFilePath);
            encode(uploadAsset);

        }
        public static void encode(IAsset uploadAsset)
        {

            
            var encodeAssetId = uploadAsset.Id; 

            var encodingPreset = "H264 Multiple Bitrate 720p";
            var assetToEncode = context.Assets.Where(a => a.Id == encodeAssetId).FirstOrDefault();
            if (assetToEncode == null)
            {
                throw new ArgumentException("Could not find assetId: " + encodeAssetId);
            }

            IJob job = context.Jobs.Create("Encoding " + assetToEncode.Name + " to " + encodingPreset);

            IMediaProcessor latestWameMediaProcessor = (from p in context.MediaProcessors where p.Name == "Media Encoder Standard" select p).ToList().OrderBy(wame => new Version(wame.Version)).LastOrDefault();
            ITask encodeTask = job.Tasks.AddNew("Encoding", latestWameMediaProcessor, encodingPreset, TaskOptions.None);
            encodeTask.InputAssets.Add(assetToEncode);
            encodeTask.OutputAssets.AddNew(assetToEncode.Name + " as " + encodingPreset, AssetCreationOptions.None);
            encodeTask.Configuration = File.ReadAllText(Environment.CurrentDirectory + "\\" + "media_encoding_config.xml");
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>((sender, jsc) => Console.WriteLine(string.Format("{0}\n  State: {1}\n  Time: {2}\n\n", ((IJob)sender).Name, jsc.CurrentState, DateTime.UtcNow.ToString(@"yyyy_M_d_hhmmss"))));
            job.Submit();
            job.GetExecutionProgressTask(CancellationToken.None).Wait();
            var preparedAsset = job.OutputMediaAssets.FirstOrDefault();
            publish(preparedAsset);

        }

        public static void publish(IAsset preparedAsset)
        {
            var streamingAssetId = preparedAsset.Id; // "YOUR ASSET ID";
            var daysForWhichStreamingUrlIsActive = 365;
            var streamingAsset = context.Assets.Where(a => a.Id == streamingAssetId).FirstOrDefault();
            var accessPolicy = context.AccessPolicies.Create(streamingAsset.Name, TimeSpan.FromDays(daysForWhichStreamingUrlIsActive),
                                                     AccessPermissions.Read);
            string streamingUrl = string.Empty;
            var assetFiles = streamingAsset.AssetFiles.ToList();
            
            var streamingAssetFile = assetFiles.Where(f => f.Name.ToLower().EndsWith(".ism")).FirstOrDefault();
            if (string.IsNullOrEmpty(streamingUrl) && streamingAssetFile != null)
            {
                var locator = context.Locators.CreateLocator(LocatorType.OnDemandOrigin, streamingAsset, accessPolicy);
                Uri smoothUri = new Uri(locator.Path + streamingAssetFile.Name + "/manifest");
                streamingUrl = smoothUri.ToString();
            }
            
            Console.WriteLine("Streaming Url: " + streamingUrl);

            Console.ReadLine();
        }

    }
}
