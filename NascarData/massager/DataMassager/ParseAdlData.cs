using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMassager
{
    public class ParseAdlData
    {
        public void Go(string path)
        {
            var uniqueValues = new Dictionary<string, int>();
            var rowcount = 0;
            var rowsWithValues = 0;

            Console.WriteLine($"Starting {DateTime.Now}...");
            var sw = new Stopwatch();
            sw.Start();
            using (var stream = File.OpenRead(path))
            {
                Console.WriteLine($"Reading...{DateTime.Now}");
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 102400))
                {
                    Console.CursorVisible = false;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.CursorTop = 2;
                        Console.CursorLeft = 0;
                        Console.Write($"{rowcount} rows read...");
                        rowcount++;
                        var pieces = line.Split(',').Where(x => x.Contains("u2")).ToList();
                        if (!pieces.Any()) continue;

                        rowsWithValues++;
                        Console.CursorTop = 3;
                        Console.CursorLeft = 0;
                        Console.Write($"{rowsWithValues} rows with data read...");
                        var key = pieces[0];
                        if (uniqueValues.ContainsKey(key))
                        {
                            uniqueValues[key] = uniqueValues[key] + 1;
                        }
                        else
                        {
                            uniqueValues.Add(key, 1);
                        }
                    }
                }

                Console.WriteLine($"{uniqueValues.Keys.Count} unique keys found:");
                foreach (var a in uniqueValues)
                {
                    Console.WriteLine($"{a.Key}: {a.Value} lines");
                }
                //var lines = File.ReadAllLines(path);
                //foreach (var a in lines)
                //{
                //    //"u2":"4",

                //}
            }
            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds} to read and process {rowcount} lines");
        }

        public void ParallelRead(string path)
        {
            Console.WriteLine($"Starting {DateTime.Now}...");
            var sw = new Stopwatch();
            sw.Start();

            var lines = File.ReadLines(path);
            sw.Stop();

            var d = new ConcurrentDictionary<string, int>();
            sw.Restart();
            Parallel.ForEach(lines, x =>
            {
                if (!x.Contains("u2")) return;
                var pieces = x.Split(',').Single(z => z.Contains("u2")).Split(':');
                var key = pieces[1].Replace("\"", string.Empty);
                if (d.ContainsKey(key))
                {
                    d[key] = d[key] + 1;
                }
                else
                {
                    d[key] = 1;
                }
            });
            sw.Stop();
            Console.WriteLine($"{d.Keys.Count} unique keys found in {sw.ElapsedMilliseconds}ms:");
            foreach (var a in d)
            {
                Console.WriteLine($"{a.Key}: {a.Value} lines");
            }
            //.Where(x => x.Contains("u2")).Select(y => y.Split(',').Single(z => z.Contains("u2"))).Select(a => new { Rating = a.Split(':')[0], Value = a.Split(':')[1].Replace("\"", string.Empty) });
        }

        public void ReadLines(string path)
        {
            Console.WriteLine($"Starting {DateTime.Now}...");
            var sw = new Stopwatch();
            sw.Start();

            var lines = File.ReadLines(path).Where(x => x.Contains("u2")).Select(y => y.Split(',').Single(z => z.Contains("u2"))).Select(a => new { Rating = a.Split(':')[0], Value = a.Split(':')[1].Replace("\"", string.Empty) });
            sw.Stop();

            var uniqueValues = new Dictionary<string, int>();
            var rowcount = 0;

            Console.WriteLine($"File.ReadLines took {sw.ElapsedMilliseconds}ms");

            sw.Restart();

            var totals = lines.GroupBy(x => x.Value).Select(y => new { y.Key, Count = y.Count() });
            sw.Stop();

            Console.WriteLine($"GroupBy: {sw.ElapsedMilliseconds}ms");

            sw.Restart();

            //totals = totals.OrderBy(x => x.Key);

            sw.Stop();
            Console.WriteLine($"OrderBy took {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            foreach (var a in totals)
            {
                Console.WriteLine($"{a.Key}: {a.Count}");
            }
            sw.Stop();
            Console.WriteLine($"Order + print {sw.ElapsedMilliseconds}ms");
            return;

            foreach (var l in lines)
            {
                //Console.CursorVisible = false;
                //Console.CursorTop = 2;
                //Console.CursorLeft = 0;
                //Console.Write($"{rowcount} rows read...");
                rowcount++;

                //var pieces = l.Split(',').Where(x => x.Contains("u2")).ToList();
                //if (!pieces.Any()) continue;

                //rowsWithValues++;
                //Console.CursorTop = 3;
                //Console.CursorLeft = 0;
                //Console.Write($"{rowsWithValues} rows with data read...");
                var key = l.Value;
                if (uniqueValues.ContainsKey(key))
                {
                    uniqueValues[key] = uniqueValues[key] + 1;
                }
                else
                {
                    uniqueValues.Add(key, 1);
                }
            }
            Console.WriteLine($"{uniqueValues.Keys.Count} unique keys found:");
            foreach (var a in uniqueValues)
            {
                Console.WriteLine($"{a.Key}: {a.Value} lines");
            }
            //sw.Restart();
            //lines = lines.ToList();
            sw.Stop();

            Console.WriteLine($"{rowcount} lines in {sw.ElapsedMilliseconds}ms ({rowcount / (sw.ElapsedMilliseconds / 1000d)} lines/sec)");
            //sw.Restart();
            //var totals = lines.GroupBy(y => y.Value).Select(g => new { Rating = g.Key, Count = g.Count() }).ToList();
            //sw.Stop();
            //Console.WriteLine($"GroupBy/Count took {sw.ElapsedMilliseconds}ms");

            //sw.Restart();
            //totals.OrderBy(x => x.Rating).ToList().ForEach(x => Console.WriteLine($"{x.Rating}: {x.Count}"));
            //sw.Stop();
            //Console.WriteLine($"Print {sw.ElapsedMilliseconds}ms");
        }

        public void ReadAllLines(string path)
        {
            Console.WriteLine($"Starting {DateTime.Now}...");
            var sw = new Stopwatch();
            sw.Start();

            var lines =
                File.ReadAllLines(path)
                    .Where(x => x.Contains("u2"))
                    .Select(y => y.Split(',').Single(z => z.Contains("u2")))
                    .Select(a => new { Rating = a.Split(':')[0], Value = a.Split(':')[1].Replace("\"", string.Empty) }).ToList();
            sw.Stop();

            Console.WriteLine($"File.ReadLine read {lines.Count} lines in {sw.ElapsedMilliseconds}ms ({(sw.ElapsedMilliseconds / 1000d) / lines.Count} lines/sec)");
            sw.Restart();
            var totals = lines.GroupBy(y => y.Value).Select(g => new { Rating = g.Key, Count = g.Count() }).ToList();
            sw.Stop();
            Console.WriteLine($"GroupBy/Count took {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            totals.OrderBy(x => x.Rating).ToList().ForEach(x => Console.WriteLine($"{x.Rating}: {x.Count}"));
            sw.Stop();
            Console.WriteLine($"Print {sw.ElapsedMilliseconds}ms");
        }
    }
}