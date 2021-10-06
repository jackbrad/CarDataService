using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileReformatter
{
    class Program
    {
        private static DateTime GpsTimeToUtc(double gpsTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(gpsTime);// - 17.0);
        }


        static void Main(string[] args)
        {
            ReadFile();
            Console.ReadLine();
            return;
            //var source = @"c:\temp\nascar\SV-Data-5202.log";
            //var source = @"c:\temp\nascar\SV-Data-lite.log";
            var source = @"E:\temp\nascar\30_0_0d16c1f87fd841e5aa0acff601de61e6.json";
            var f = new FileInfo(source);
            var lines = System.IO.File.ReadAllLines(source);
            Console.WriteLine($"Found {lines.Length} lines, {f.Length} bytes, {f.Length / lines.Length} bytes per line");
            //20160128-17:43:42.799
            var startTime = GpsTimeToUtc(double.Parse(lines.First().Split(';')[3]));
            var endTime = GpsTimeToUtc(double.Parse(lines.Last().Split(';')[3]));
            //var startTime = DateTime.ParseExact(lines.First().Split(';')[4], "yyyyMMdd-HH:mm:ss.fff", new CultureInfo("en-us"));
            //var endTime = DateTime.ParseExact(lines.Last().Split(';')[4], "yyyyMMdd-HH:mm:ss.fff", new CultureInfo("en-us"));
            Console.WriteLine($"Race start: {startTime} ({startTime.ToLocalTime()})");
            Console.WriteLine($"Race end: {endTime} ({endTime.ToLocalTime()})");
            var raceTime = endTime - startTime;
            Console.WriteLine($"Race duration: {raceTime}, {lines.Length} events received. {lines.LongLength / raceTime.TotalSeconds} events per second, {Math.Round((f.Length / (raceTime.TotalSeconds) / 1000), 4)}kBps");

            //ToJsonObjectStrings(lines, f.Length, raceTime.TotalSeconds);
            Console.WriteLine("All finished.");
            Console.ReadLine();
        }

        private static void ReadFile()
        {
            var source = @"E:\temp\nascar\30_0_0d16c1f87fd841e5aa0acff601de61e6.json";
            var f = new FileInfo(source);
            var lines = System.IO.File.ReadAllLines(source);
            Console.WriteLine($"Found {lines.Length} lines, {f.Length} bytes, {f.Length / lines.Length} bytes per line");
            lines = lines.Where(x => x.Contains("GPSTime")).ToArray();
            Console.WriteLine($"Using {lines.Length} lines, {f.Length} bytes, {f.Length / lines.Length} bytes per line");
            var timeDifferences = new List<long>();
            foreach (var l in lines)
            {
                var line = JObject.Parse(l);
                var gpsTime = line["GPSTime"].ToString();
                var gpsString = line["GPSString"].ToString();
                var eventTime = line["EventEnqueuedUtcTime"].ToString();
                var gpsTimeStamp = GpsTimeToUtc(double.Parse(gpsTime));
                var eventTimeStamp = DateTime.Parse(eventTime);
                var diff = eventTimeStamp - gpsTimeStamp;
                timeDifferences.Add(diff.Ticks);
            }

            var averageTicks = timeDifferences.Average();
            Console.WriteLine($"Average time deviation: {Math.Round(averageTicks, 4)} ticks ({Math.Round(averageTicks / 10000, 4)}ms) over {timeDifferences.Count} samples");
        }

        private static void ToJsonObjectStrings(string[] stuff, long originalSize, double raceTime)
        {
            var types = new List<Type>() { typeof(FullHeader), typeof(SingleCharacterHeader) };
            var ops = new List<Func<string, object, List<string>, string>>() { Json, Csv };
            var sizes = (from t in types from op in ops select Go(t, stuff, op)).ToList();

            Console.WriteLine("=== result ===");
            foreach (var a in sizes.OrderBy(x => x.Size))
            {
                Console.WriteLine($"{a.Function} with {a.Type}: {a.Size} bytes - {Math.Round(((double)a.Size / (double)originalSize) * 100, 2)}% vs original, {Math.Round(((double)a.Size / (raceTime)) / 1000, 4)}kBps");
            }
        }

        private static Result Go(Type t, string[] stuff, Func<string, object, List<string>, string> thingToDo)
        {
            var sb = new StringBuilder();
            Console.Write($"Writing data to {thingToDo.Method.Name} using {t.Name} type...");
            var linesConverted = 0;
            var thing = Activator.CreateInstance(t);
            var ignoreList = new List<string>() { "GPSString" };
            using (var stream = new FileStream($@"C:\temp\nascar\sv-data-{thingToDo.Method.Name}-{t.Name}.json", FileMode.Create))
            {
                var offset = 0;
                foreach (var singleRowString in stuff.Select(x => thingToDo(x, thing, ignoreList)).Where(singleRowString => !string.IsNullOrEmpty(singleRowString)))
                {
                    var data = Encoding.UTF8.GetBytes(singleRowString);
                    stream.Write(data, 0, data.Length);
                    offset = offset + data.Length;
                    //sb.Append(singleRowString);
                    linesConverted++;
                    //stream.FlushAsync();
                }
                stream.Flush();
            }

            var fi = new FileInfo($@"C:\temp\nascar\sv-data-{thingToDo.Method.Name}-{t.Name}.json");

            //File.WriteAllText($@"C:\temp\nascar\sv-data-{thingToDo.Method.Name}-{t.Name}.json", sb.ToString());
            Console.WriteLine($"wrote { linesConverted } lines. Size: { fi.Length } bytes");
            return new Result() { Function = thingToDo.Method.Name, Type = t.Name, Size = fi.Length };
        }

        private static string Json(string data, object pd, List<string> ignoreList)
        {
            var props = pd.GetType().GetFields().Where(x => !ignoreList.Contains(x.Name)).ToList();
            data = data.Replace("$NMGT;", string.Empty);
            var stuff = data.Split(';');
            if (stuff.Length < props.Count) return string.Empty;
            //var pd = new T();
            var i = 0;
            foreach (var p in props)
            {
                p.SetValue(pd, stuff[i]);
                i++;
            }
            return JsonConvert.SerializeObject(pd, Formatting.None);
        }

        private static string Csv(string data, object pd, List<string> ignoreList)
        {
            var thing = new
            {
                CarNumber = "",

            };


            var props = pd.GetType().GetFields().Where(x => !ignoreList.Contains(x.Name)).ToList();
            data = data.Replace("$NMGT;", string.Empty);
            var headerRow = string.Join(",", props.Select(x => x.Name));
            var csv = data.Replace(';', ',');
            if (csv.Split(',').Length >= props.Count) return $"{headerRow}\r\n{csv}r\n";
            var delta = props.Count - csv.Split(',').Length;
            for (var i = 0; i < delta; i++)
            {
                csv = csv + ",";
            }
            return $"{headerRow}\r\n{csv}\r\n";
        }
    }

    public struct Result
    {
        public string Type { get; set; }
        public string Function { get; set; }
        public long Size { get; set; }
    }

    //["Car Number","Vitc (Timecode)","GPS Raw","GPS String","Latitude","Longitude","Altitude","Roll","Northing","Easting","CourseHeading","VelocityHeading","Lap Number","Speed","RPM","Throttle","Brake","Lateral Acceleration","Longitudinal Acceleration"]
    public class FullHeader
    {
        public string CarNumber;
        public string Vitc;
        public string GpsRaw;
        public string GpsString;
        public string Latitude;
        public string Longitude;
        public string Altitude;
        public string Roll;
        public string Northing;
        public string Easting;
        public string CourseHeading;
        public string VelocityHeading;
        public string LapNumber;
        public string Speed;
        public string RPM;
        public string Throttle;
        public string Brake;
        public string LateralAcceleration;
        public string LongitudalAcceleration;
    }

    //public class FullHeader
    //{
    //    "CarNumber","Vitc","GPSRaw","GPSString","Latitude","Longitude","Altitude","Roll","Northing","Easting","CourseHeading","VelocityHeading","LapNumber","Speed","RPM","Throttle","Brake","LateralAcceleration","LongitudalAcceleration"
    //}

    public class SingleCharacterHeader
    {
        public string a;
        public string b;
        public string c;
        public string d;
        public string e;
        public string f;
        public string g;
        public string h;
        public string i;
        public string j;
        public string k;
        public string l;
        public string m;
        public string n;
        public string o;
        public string p;
        public string q;
        public string r;
        public string s;
    }
}
