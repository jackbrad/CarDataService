using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Nascar.EventProcessorHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var snip = 50;
            const string data = @"41,47638.972360,1456078441.400000,20160121-18:14:01.400,29.188351,-81.071387,-18.970000,0.102194,1764783.878814,633388.461728,0.000000,31.289063,0,0.000000,0,50,0,0.000000,0.000000";
            byte[] compressed;

            var originalData = new MemoryStream(Encoding.UTF8.GetBytes(data));
            using (var gzStream = new MemoryStream())
            {
                using (var gz = new GZipStream(gzStream, CompressionMode.Compress, false))
                {
                    originalData.CopyTo(gz);
                }
                compressed = gzStream.ToArray();
            }

            Console.WriteLine($"Original size: {originalData.Length}");
            Console.WriteLine($"Compressed size: {compressed.Length}");

            var wiredata = Convert.ToBase64String(compressed);
            Console.WriteLine($"compressed base64 {wiredata.Substring(0, snip)} /snip");

            string outString;
            var output = Convert.FromBase64String(wiredata);
            using (var targetStream = new MemoryStream(output))
            {
                using (var gzout = new GZipStream(targetStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzout, Encoding.UTF8))
                    {
                        outString = reader.ReadToEnd();
                    }
                }
            }

            Console.WriteLine($"{outString.Substring(0, snip)}.../snip");
            Console.WriteLine($"Original: {data.Substring(0, snip)}... /snip");
            Console.WriteLine($"Match? {data == outString}");
            Console.ReadLine();
        }
    }
}