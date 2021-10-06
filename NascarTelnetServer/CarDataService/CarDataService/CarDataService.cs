using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;

using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Xml.Linq;
using System.Linq;
using CarDataService.Subscribers;

namespace CarDataService
{
    using Formatters;
    using System;

    internal class CarDataService : TelemetryServiceBase
    {
        public List<IDisposable> Unsubscribers = new List<IDisposable>();
        private CarStreamReader _csr;

        public void RunFromConsole()
        {
            OnStart(new[] { "" });
            Console.ReadLine();
            OnStop();
        }

        private static void Main(string[] args)
        {
            //running under the service manager
            var servicesToRun = new System.ServiceProcess.ServiceBase[] { new CarDataService() };
            //used to run under console. 
            //((CarDataService)servicesToRun[0]).RunFromConsole();
            System.ServiceProcess.ServiceBase.Run(servicesToRun);
        }

        private void InitializeComponent()
        {
            ServiceName = "CarDataService";
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ReadSvStream();
                Log.Info($"Service started - User environment interactive {Environment.UserInteractive}");
            }
            catch (Exception e)
            {
                Log.Event("Service-Start-Exception");
                Log.Error(e);
            }
        }

        private void GetConfigChanges()
        {
            try
            {
                //try to get config from Azure.
                var root = XElement.Load(GetConfigFromAzure());
                var appsetts = root.Descendants(XName.Get("appSettings")).Descendants((XName.Get("add")));
                var constrings = root.Descendants(XName.Get("connectionStrings")).Descendants((XName.Get("add")));

                //gather azure appsetttings
                var appsettings = from e in appsetts select new { Key = e.Attribute(XName.Get("key")).Value, Value = e.Attribute(XName.Get("value")).Value, };
                //gather azure connectionstrings;
                var cons = from e in constrings select new { Name = e.Attribute(XName.Get("name")).Value, connectionString = e.Attribute(XName.Get("connectionString")).Value, };

                //open config for updates. 
                var cm = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                //update appsettings
                appsettings.ToList().ForEach(app =>
                {
                    if (cm.AppSettings.Settings[app.Key] != null)
                    {
                        cm.AppSettings.Settings[app.Key].Value = app.Value;
                    }
                    else
                    {
                        cm.AppSettings.Settings.Add(app.Key, app.Value);
                    }

                });

                //update connectionstrings
                cons.ToList().ForEach(cns =>
                {
                    if (cm.ConnectionStrings.ConnectionStrings[cns.Name] != null)
                    {
                        cm.ConnectionStrings.ConnectionStrings[cns.Name].ConnectionString = cns.connectionString;
                    }
                    else
                    {
                        cm.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(cns.Name, cns.connectionString));
                    }

                });

                //save and load the new settings
                cm.Save();
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.RefreshSection("connectionStrings");

            }
            catch (Exception e)
            {
                //report
                Log.Info("Failed to get appsettings config from Azure using local settings" + e.Message);
            }
        }

        private string GetConfigFromAzure()
        {
            Log.Info("Checking for Azure appsetting changes");
            //load config else where if possible. 
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["AzureConfigDir"]);
            var blockBlob = container.GetBlockBlobReference(ConfigurationManager.AppSettings["ConfigFileName"]);
            var file = AppDomain.CurrentDomain.BaseDirectory + "AzureAppSettings.config";
            blockBlob.DownloadToFile(file, FileMode.Create);
            Log.Info("Downloaded appsettings from Azure");
            return file;

        }

        private void ReadSvStream()
        {
            //load processing objects
            _csr = new CarStreamReader();
            _csr.Connected += Csr_Connected;
            _csr.Disconnected += Csr_Disconnected;

            //load the connection data for the 
            var server = ConfigurationManager.AppSettings["DataStreamingHost"];
            var port = ConfigurationManager.AppSettings["StreamingPort"];

            //start the process of stream monitoring.
            Task.Run(() =>
            {
                var dm = new DelayManager(Log);

                //terminate with too many errors
                while (!dm.ShouldTerminate(DelayType.Exception))
                {
                    try
                    {
                        _csr.Start(server, Convert.ToInt32(port));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        //delay for the time 
                        Task.Delay(dm.GetNextDelayMilliseconds(DelayType.Exception));

                    }
                }
            }
           );
        }

        private void Csr_Disconnected(object sender, EventArgs e)
        {
            // <new> added call to oncompleted on subscribers to allow graceful shutdown
            // 
            Unsubscribers.ForEach(x => ((Unsubscriber)x).Complete());
        }

        private void Csr_Connected(object sender, EventArgs e)
        {
            Unsubscribers.ForEach(x => x.Dispose());

            var csr = (CarStreamReader)sender;
            var evh = new EventHubSubscriber(new Csv());
            var lfw = new LogFileSubscriber(ConfigurationManager.AppSettings["LocalLogFilePath"]);
            var sql = new SqlSubscriber(ConfigurationManager.AppSettings["SqlSubscriber.ConnectionString"]);

            evh.ConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
            evh.EventHubName = ConfigurationManager.AppSettings["EventHubName"];
            ////used to queue a session start message to Azure
            evh.SessionStarted += Evh_SessionStarted;
            ////subscribe writers save the unsubscribers for later. 
            Unsubscribers.Add(csr.Subscribe(evh));
            Unsubscribers.Add(csr.Subscribe(lfw));
            Unsubscribers.Add(csr.Subscribe(sql));
        }

        private void Evh_SessionStarted(object sender, EventArgs e)
        {
            var msg = ((NewSessionEventArgs)e).Message;
            //needed: sessionkey, GPSTIME, LAT, LON, ALT, DATETIMEUTCNow
            QueueSessionMsg(msg);
        }

        private static void QueueSessionMsg(string msg)
        {
            //open cloud storage queue
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("sportsvisionsessionqueue");
            queue.CreateIfNotExists();

            //encode and send the message;
            var message = new CloudQueueMessage(msg);
            queue.AddMessage(message);
        }

        protected override void OnStop()
        {
            Log.Info("Service stopped");
            Log.Flush();
        }
    }
}