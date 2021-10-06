using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NascarTelnetClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Client();
            c.Go();
            Console.ReadLine();
        }

        public class Client
        {
            public void Go()
            {
                var t = new TcpClient(ConfigurationManager.AppSettings["Host"], int.Parse(ConfigurationManager.AppSettings["Port"]));

                var stream = t.GetStream();
                //while (stream.DataAvailable)
                //{
                //    var bytes = stream.Read(data, 0, data.Length);
                //    if (bytes == 0)
                //    {
                //        //no data
                //        //?
                //    }
                //    sbuffer.Clear();
                //    sbuffer.Append(Encoding.ASCII.GetString(data, 0, bytes));
                //    _lastrcvd = DateTime.Now;

                //    mb.AddData(sbuffer.ToString()).ForEach(m =>
                //    {
                //        sbuffer.Clear().Append(m.Trim());
                //        _observers.ForEach(x => x.OnNext(sbuffer.ToString()));
                //    }
                //        );
                //}



                var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(ConfigurationManager.AppSettings["Host"], int.Parse(ConfigurationManager.AppSettings["Port"]));
                var p = new LineParser();
                while (true)
                {
                    var buffer = new byte[128];
                    s.Receive(buffer);
                    var lines = p.ParseChunk(Encoding.ASCII.GetString(buffer));
                    lines.ForEach(Console.WriteLine);
                }
            }
        }

        public class LineParser
        {
            private readonly List<string> _buffer = new List<string>();

            public List<string> ParseChunk(string chunk)
            {
                if (_buffer.Any())
                {
                    chunk = $"{_buffer.First()}{chunk}";
                }

                var lines = chunk.Split("$".ToCharArray()).Where(x => x.Trim().Length > 0).Select(y => "$" + y).ToList();

                _buffer.Clear();

                if (lines.Last().IndexOf("\r\n", StringComparison.Ordinal) != -1) return lines;

                var carryoverLine = lines.Last();
                _buffer.Add(carryoverLine);
                lines.Remove(carryoverLine);
                return lines;
            }
        }
    }
}