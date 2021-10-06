using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataMassager
{
    public class SessionFixer
    {
        
    }



    public class RunLog
    {
        private const string Columns = @"SessionId,Car,Vitc,GPSTime,GPSString,Lat,Lon,Alt,Roll,North,East,CourseHead,VelocityHead,Lap,Speed,RPM,Throttle,Brake,LatAccel,LongAccel,u0,u1,u2";
        //$NMGT;47;45812.817592;1456681426.800000;20160128-17:43:46.799;33.384538;-84.314589;227.330000;0.235864;1231149.654334;2251432.094192;0.000000;-120.234375;56;0.068359;0;100;0;0.000000;0.000000;0.894
        private readonly long _sessionId = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 100;
        private readonly int _fieldCount = Columns.Split(',').Length;
        private readonly string _outputPath = $@"e:\temp\nascar\temp-{DateTime.UtcNow.ToString("O").Replace(":", "-")}.csv";
        private readonly Queue _q = new Queue(false);

        public void Go(string path)
        {
            Console.WriteLine($"Reading {path}...");
            Console.CursorVisible = false;
            var swTotal = new Stopwatch();
            var sw = new Stopwatch();
            sw.Start();
            swTotal.Start();
            Console.WriteLine($"Session ID: {_sessionId}");
            var cb = new ConcurrentBag<string>();
            //Parallel.ForEach(File.ReadLines(path), line =>
            //{
            //    cb.Add(ProcessLine(line));
            //});
            var i = 0;
            foreach (var a in File.ReadLines(path))
            {
                Console.CursorLeft = 0;
                Console.Write($"Sending line {i}");
                //Task.Delay(50);
                ProcessLine(a);
                i++;
            }
            Console.WriteLine();

            sw.Stop();

            Console.WriteLine($"Finished parsing in {sw.ElapsedMilliseconds}ms, writing...");
            //var data = cb.ToArray();
            //File.WriteAllLines(_outputPath, data);

            swTotal.Stop();
            var length = i * 1d;
            var length2 = length * 2d;
            var length3 = length + length2;
            var process = sw.ElapsedMilliseconds;
            var total = swTotal.ElapsedMilliseconds;
            Console.WriteLine($"took {process}ms to read {length} rows ({length / process * 1000} rows/sec), or {(process / length) * 1000000}ns/row");
            Console.WriteLine($"took {total - process}ms to project and write {length2} rows ({length2 / (total - process) * 1000} rows/sec), or {((total - process) / length2) * 1000000}ns/row");
            Console.WriteLine($"took {total}ms to process and write {length3} rows ({length3 / total * 1000} rows/sec), or {(total / length3) * 1000000}ns/row");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ProcessLine(string l)
        {
            var line = l.Replace("$NMGT", _sessionId.ToString()).Replace(";", ",");

            var items = line.Split(',');
            var delta = _fieldCount - items.Length;
            var col = Columns;

            if (delta > 0) // add empty vals
            {
                var ab = string.Join(",0", new string[delta + 1]);
                line = line + ab;
            }

            if (delta >= 0)
            {
                SendToEventHub($"{col}\r\n{line}");
                return $"{col}\r\n{line}";
            }

            for (var i = 0; i < Math.Abs(delta); i++)
            {
                col = $"{col},u{i}";
            }

            SendToEventHub($"{col}\r\n{line}");
            return $"{col}\r\n{line}";
        }

        private void SendToEventHub(string row)
        {
            _q.WriteToHub(row);
        }
    }
}