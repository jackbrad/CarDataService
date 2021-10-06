using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace PowerBiRealTime
{
    internal class Program
    {
        private const string DatasetsUri = "https://api.powerbi.com/beta/myorg";
        private const string DatasetName = "Nascar-API";

        static void Main(string[] args)
        {
            var tokensrv = new AzureAdToken(new TokenServiceConfiguration()
            {
                Authority = "https://login.windows.net/jpd.ms/oauth2/authorize",
                ClientId = "6455de0f-a3b8-4184-8585-79d7547ef1e7",
                ClientSecret = "putVwbb3duD3s5ORqAuGBoI9wDD4LY/NAdEm3ZyfYbo=",
                RedirectUri = new Uri("http://apps.jpd.ms/dev/powerbi-pusher"),
                Resource = "https://analysis.windows.net/powerbi/api",
                ResourceId = "00000009-0000-0000-c000-000000000000",
                TokenCache = new TokenCache()
            });

            var nativetokensrv = new AzureAdToken(new TokenServiceConfiguration()
            {
                Authority = "https://login.windows.net/jpd.ms/oauth2/authorize",
                ClientId = "815aadaf-a98d-45ca-986b-0176058003c8",
                ClientSecret = "Cld/OQfRP/RuqWhld7xy5/AYpnXbMF9O2xcQiRwYqLE=",
                RedirectUri = new Uri("https://apps.jpd.ms/native/pbi"),
                Resource = "https://analysis.windows.net/powerbi/api",
                ResourceId = "00000009-0000-0000-c000-000000000000",
                TokenCache = new TokenCache()
            });

            var headless = new AzureAdHeadlessToken(new TokenServiceConfiguration()
            {
                Authority = "https://login.microsoftonline.com/jpd.ms/oauth2/token",
                ClientId = "6455de0f-a3b8-4184-8585-79d7547ef1e7",
                ClientSecret = "putVwbb3duD3s5ORqAuGBoI9wDD4LY/NAdEm3ZyfYbo=",
                RedirectUri = new Uri("http://apps.jpd.ms/dev/powerbi-pusher"),
                Resource = "https://analysis.windows.net/powerbi/api",
                ResourceId = "00000009-0000-0000-c000-000000000000",
                TokenCache = new TokenCache(),
                User = "john@jpd.ms",
                Password = "Chapter22"
            });

            var nativeheadless = new AzureAdHeadlessToken(new TokenServiceConfiguration()
            {
                Authority = "https://login.microsoftonline.com/jpd.ms/oauth2/authorize",
                ClientId = "815aadaf-a98d-45ca-986b-0176058003c8",
                ClientSecret = "Cld/OQfRP/RuqWhld7xy5/AYpnXbMF9O2xcQiRwYqLE=",
                RedirectUri = new Uri("https://apps.jpd.ms/native/pbi"),
                Resource = "https://analysis.windows.net/powerbi/api",
                ResourceId = "00000009-0000-0000-c000-000000000000",
                TokenCache = new TokenCache(),
                User = "john@jpd.ms",
                Password = "Chapter22"

            });

            var msftnativeheadless = new AzureAdHeadlessToken(new TokenServiceConfiguration()
            {
                Authority = "https://login.microsoftonline.com/common/oauth2/authorize",
                ClientId = "79669f64-bcb9-491a-bf01-429bc574b79a",
                ClientSecret = "Cld/OQfRP/RuqWhld7xy5/AYpnXbMF9O2xcQiRwYqLE=",
                RedirectUri = new Uri("https://johndand/pbi-native"),
                Resource = "https://analysis.windows.net/powerbi/api",
                ResourceId = "00000009-0000-0000-c000-000000000000",
                TokenCache = new TokenCache(),
                User = "johndand@microsoft.com",
                Password = "Chapter2334"

            });

            var msftnative = new AzureAdToken(new TokenServiceConfiguration()
            {
                Authority = "https://login.microsoftonline.com/common/oauth2/authorize",
                ClientId = "79669f64-bcb9-491a-bf01-429bc574b79a",
                ClientSecret = "Cld/OQfRP/RuqWhld7xy5/AYpnXbMF9O2xcQiRwYqLE=",
                RedirectUri = new Uri("https://johndand/pbi-native"),
                Resource = "https://analysis.windows.net/powerbi/api",
                ResourceId = "00000009-0000-0000-c000-000000000000",
                TokenCache = new TokenCache(),
                User = "johndand@microsoft.com",
                Password = "Chapter2334"
            });

            var pbi = new PbiDataMover(new DatasetHelper(DatasetsUri, DatasetName, nativeheadless));

            //$NMGT;15;47601.007262;1456078403.400000;20160121-18:13:23.400;29.189462;-81.070074;-18.820000;-1.#IND00;1765187.801960;633807.690807;0.000000;-176.835938;0;0.205078;0;100;0;0.000000;0.000000
            Console.WriteLine($"Getting data from file; skipping first 50k rows");
            var f = System.IO.File.ReadAllLines(@"e:\temp\nascar\Cup-Data-5202.log").Skip(500000);
            foreach (var l in f)
            {
                lock (pbi.HotData)
                {
                    pbi.HotData.Add(WorkData.ToObject<CarData>(l));
                }
                if (pbi.HotData.Count > 500)
                {
                    pbi.Send();
                    break;
                }
            }

            Console.ReadLine();
        }
    }

    public static class WorkData
    {
        public static T ToObject<T>(string data) where T : new()
        {
            var value = new T();
            var dataArray = data.Replace("$NMGT;", string.Empty).Split(';');
            var i = -1;
            foreach (var p in typeof(T).GetFields())
            {
                i++;
                var dataValue = dataArray[i];
                if (dataValue.IndexOf("#", StringComparison.Ordinal) > -1) //zero out bad fields, but preserve the row
                {
                    dataValue = "0";
                }
                switch (p.FieldType.FullName)
                {
                    case "System.Int32":
                        {
                            p.SetValue(value, int.Parse(dataValue));
                            continue;
                        }
                    case "System.Double":
                        {
                            p.SetValue(value, double.Parse(dataValue));
                            continue;
                        }
                    case "System.DateTime":
                        {
                            p.SetValue(value, DateTime.Parse(dataValue));
                            continue;
                        }
                    default:
                        {
                            p.SetValue(value, dataValue);
                            continue;
                        }
                }
            }
            return value;
        }

        public static string Csv(string data, List<string> fields, List<string> ignoreList)
        {
            var badData = data.IndexOf("#") > -1;
            var props = fields.Where(x => !ignoreList.Contains(x)).ToList();
            data = data.Replace("$NMGT;", string.Empty).Replace("{m:", string.Empty).Replace("}", string.Empty);
            var headerRow = string.Join(",", props);
            var csv = data.Replace(';', ',').Split(',');
            if (badData)
            {
                var working = csv.ToList();
                var bad = working.Where(x => x.Contains("#")).ToList();
                foreach (var b in bad)
                {
                    var index = working.IndexOf(b);
                    csv[index] = "0";
                }
            }
            csv[3] = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(csv[2]) - 17.0).ToString("O"); //17 sec for GPS/UTC offset
            if (csv.Length > props.Count) //borked, return empty
            {
                var vals = new string[props.Count];
                for (var i = 0; i < vals.Length; i++)
                {
                    vals[i] = "0";
                }
                var emptyRow = string.Join(",", vals);
                return $"{headerRow}\r\n{vals}\r\n";
            }
            var csvString = string.Join(",", csv);
            if (csv.Length == props.Count) return $"{headerRow}\r\n{csvString}\r\n";

            var delta = props.Count - csv.Length;
            for (var i = 0; i < delta; i++)
            {
                csvString = csvString + ",";
            }
            return $"{headerRow}\r\n{csvString}\r\n";
        }
    }

    public class PbiDataMover
    {
        private readonly DatasetHelper _dsh;
        public List<CarData> HotData = new List<CarData>();

        public PbiDataMover(DatasetHelper dsh)
        {
            Console.WriteLine("let's go");
            _dsh = dsh;
            Startup<CarData>(true);
        }
        private void Startup<T>(bool destructive = false) where T : new()
        {
            if (destructive)
            {
                _dsh.DeleteDataset();
            }
            _dsh.CreateDataset<T>();
        }

        public void SendRows(IEnumerable<CarData> newData, string tableName)
        {
            Console.WriteLine($"Adding data to table {tableName}, deleting rows first...");
            _dsh.DeleteRows(tableName);
            var sendData = GetLatestForEachRow();
            _dsh.AddRows(tableName, sendData);
            lock (HotData)
            {
                HotData.Clear();
            }
        }

        public void Send()
        {
            SendRows(HotData, "CarData");
        }

        public List<CarData> GetLatestForEachRow()
        {
            var sw = new Stopwatch();
            sw.Start();
            lock (HotData)
            {
                var cars = HotData.Select(x => x.CarNumber).Distinct();
                var data = cars.Select(c => HotData.Where(x => x.CarNumber == c).OrderByDescending(y => y.ArrivalTime).First()).ToList();
                sw.Stop();
                Console.WriteLine($"Race time: {HotData.First().ReportedTime}. took {sw.ElapsedMilliseconds}ms to get {data.Count} rows out of {HotData.Count} rows");
                return data;
            }
        }
    }
}