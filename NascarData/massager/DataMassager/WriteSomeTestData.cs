using System;
using System.Configuration;
using System.IO;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace DataMassager
{
    public class WriteSomeTestData
    {
        private readonly EventHubClient _eh;

        public void Go(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var lines = File.ReadAllLines(path);
            //var a = lines.Where(x => x.StartsWith("{\"a\":")).ToList();
            //var offset = lines.Where(x => x.StartsWith("{\"Car\":")).ToList();
            //var nmgt = lines.Where(x => x.StartsWith("{\"a\":\"NMGT\"")).ToList();
            //var nmgt2 = lines.Where(x => x.ToLower().Contains("nmgt")).ToList();
            Console.WriteLine($"{fileName}: Total lines: {lines.Length}");
            var sent = 0;
            Console.ForegroundColor = ConsoleColor.Yellow;
            var q = new Queue();
            foreach (var l in lines)
            {
                if (l.Contains("NMGT")) continue;
                //Console.WriteLine($"Modifying {l}...");
                var data = GetEventData(l);
                q.WriteToHub(data);
                sent++;
                Console.CursorLeft = 0;
                Console.Write($"{sent} rows sent...");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All done.");
            //File.WriteAllLines($@"e:\temp\nascar\nh-fixed\a-{fileName}.json", a);
            //File.WriteAllLines($@"e:\temp\nascar\nh-fixed\offset-{fileName}.json", offset);
        }

       

        //{"a":"146861083327300","b":"44","c":"55616.818544","d":"1468610832.800000","e":"20160715-19:27:12.799","f":"43.364047","g":"-71.459467","h":"120.920000","i":"-1.#IND00","j":"314979.253711","k":"1039350.298122","l":"64.335938","m":"21.445313","n":"9","o":"0.000000","p":"0","q":"7","r":"0","s":"0.000000","t":"0.000000","u":"9.612025","EventProcessedUtcTime":"2016-07-15T19:27:17.8246901Z","PartitionId":6,"EventEnqueuedUtcTime":"2016-07-15T19:27:15.4870000Z"}
        //Car;Vitc;GPSTime;GPSString;Lat;Lon;Alt;Roll;North;East;CourseHead;VelocityHead;Lap;Speed;RPM;Throttle;Brake;LatAccel;LongAccel
        private byte[] GetEventData(string row)
        {
            //var newRow = row.
            //    Replace("\"a\"", "SessionId").
            //    Replace("\"b\"", "Car").
            //    Replace("\"c\"", "Vitc").
            //    Replace("\"d\"", "GPSTime").
            //    Replace("\"e\"", "GPSString").
            //    Replace("\"f\"", "Lat").
            //    Replace("\"g\"", "Lon").
            //    Replace("\"h\"", "Alt").
            //    Replace("\"i\"", "Roll").
            //    Replace("\"j\"", "North").
            //    Replace("\"k\"", "East").
            //    Replace("\"l\"", "CourseHead").
            //    Replace("\"m\"", "VelocityHead").
            //    Replace("\"n\"", "Lap").
            //    Replace("\"o\"", "Speed").
            //    Replace("\"p\"", "RPM").
            //    Replace("\"q\"", "Throttle").
            //    Replace("\"r\"", "Brake").
            //    Replace("\"s\"", "LatAccel").
            //    Replace("\"t\"", "LongAccel");
            return Encoding.UTF8.GetBytes(row);
        }
    }
}