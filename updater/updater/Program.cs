using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Xml.Linq;



namespace updater
{
    class Program
    {
        static void Main(string[] args)
        {

            string sn = ConfigurationManager.AppSettings["ServiceName"];
            string _newfilesDir = ConfigurationManager.AppSettings["UpdatedFilesDirectory"];
            string _oldfilesDir = ConfigurationManager.AppSettings["ApplicatonDirectory"];

            Console.WriteLine($"Updating service: {sn}");
            Console.WriteLine($"New Files- {_newfilesDir}");
            Console.WriteLine($"Old Files- {_oldfilesDir}");

            //checking for downloads. 
            Console.WriteLine("Checking for New files...") ;
            if (NewVerOnBlob(LastLoadedVersion()))
            {
                //get new files. 
                DownloadNewFiles();
                Console.WriteLine($"Stopping {sn}");
                StopService(sn, 60000);
                MoveFiles(_newfilesDir, _oldfilesDir);
                Console.WriteLine($"Starting {sn}");
                StartService(sn, 60000);
            }
            
            
            Console.WriteLine($"Complete");


        }
        private static bool NewVerOnBlob(string LastVersionNum)
        {
            
            //load config else where if possible. 
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["AzureConfigDir"]);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(ConfigurationManager.AppSettings["ConfigFileName"]);
            string file = AppDomain.CurrentDomain.BaseDirectory + "AzureAppSettings.config";
            blockBlob.DownloadToFile(file, FileMode.Create);
            XElement root = XElement.Load(file);
            var appsetts = root.Descendants(XName.Get("appSettings")).Descendants((XName.Get("add")));
            //gather azure appsetttings
            var appsettings = from e in appsetts
                              where e.Attribute(XName.Get("key")).Value== "LatestVer"
                              select new { Key = e.Attribute(XName.Get("key")).Value, Value = e.Attribute(XName.Get("value")).Value, };
            return appsettings.FirstOrDefault().Value != LastVersionNum;                     
        }
        private static string LastLoadedVersion()
        {
            string file = AppDomain.CurrentDomain.BaseDirectory + "AzureAppSettings.config";
            XElement root = XElement.Load(file);
            var appsetts = root.Descendants(XName.Get("appSettings")).Descendants((XName.Get("add")));
            //gather azure appsetttings
            var appsettings = from e in appsetts
                              where e.Attribute(XName.Get("key")).Value == "LatestVer"
                              select new { Key = e.Attribute(XName.Get("key")).Value, Value = e.Attribute(XName.Get("value")).Value, };
           
            return appsettings.FirstOrDefault().Value;
        }

        private static void DownloadNewFiles()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["AzureConfigDir"]);
            Directory.CreateDirectory("temp");
            container.ListBlobs(null, false).ToList().ForEach(b => {
                
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(Path.GetFileName(b.Uri.ToString()));
                blockBlob.DownloadToFile(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\" + Path.GetFileName(b.Uri.ToString()), FileMode.Create);
                Console.WriteLine($"Downloading - {b.Uri.ToString()}");
            });


        }


        public static void MoveFiles(string fromDir, string toDir)
        {
            Directory.GetFiles(fromDir).ToList().ForEach(f =>
                {
                    Console.WriteLine($"Copying: {f}");
                    File.Copy(f,Path.Combine(toDir,Path.GetFileName(f)), true);
                });
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            try
            {
                ServiceController service = new ServiceController(serviceName);
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    service.Stop();
                }
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }
    }
}
