using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace CarDataService
{
    internal class CarStreamReader : TelemetryBase, IAsyncObservable<string>
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        private readonly List<IAsyncObserver<string>> _observers;
        private TcpClient _client;

        public long SessionId { get; set; }

        public CarStreamReader() : base()
        {
            _observers = new List<IAsyncObserver<string>>();
        }

        private bool Connect(string server, int port)
        {
            try
            {
                _client = new TcpClient(server, port);
                var prop = new Dictionary<string, string>() { { "Server", server }, { "Port", port.ToString() } };
                Log.Event("TcpClient-Connected", prop);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public void Start(string server, int port)
        {
            do
            {
                var connected = Connect(server, port);

                if (connected)
                {
                    SessionId = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 100;
                    Connected?.Invoke(this, new NewSessionEventArgs(SessionId.ToString()));
                    ReadStream();
                }
                Thread.Sleep(10000);
            } while (true);
        }

        private void ReadStream()
        {
            var data = new byte[256];
            GpsParser p = new GpsParser();
            p.RecordParsed += RecordParsedHandler;
            // <test>
            //var sbuffer = new StringBuilder(256);
            //var mb = new MessageBuffer();
            var contCount = 0;

            try
            {
                while (_client.Connected)
                {
                    var stream = _client.GetStream();
                    if (!stream.DataAvailable)
                    {
                        //If data is not available in roughly 15 seconds.. disconnect
                        if (contCount > 15)
                        {
                            EndSession();
                            return;
                        }

                        Thread.Sleep(1000);
                        contCount++;
                        continue;
                    }

                    contCount = 0;
                    var bytes = stream.Read(data, 0, data.Length);

                    // send the bytes to the parser
                    //
                    p.ParseBytes(data, bytes);

                    //
                    //sbuffer.Clear();
                    //sbuffer.Append(Encoding.ASCII.GetString(data, 0, bytes));

                    ////todo: parse this to a real object? maybe if we foresee lots of downstream subscribers in the future
                    //mb.AddData(sbuffer.ToString()).ForEach(m =>
                    //{
                    //    sbuffer.Clear().Append(m.Trim());
                    //    //todo: this is going to move to Async soon
                    //    Parallel.ForEach(_observers, x =>
                    //    {
                    //        x.OnNext($"{SessionId.ToString()};{sbuffer.ToString()}");
                    //    });
                    //});
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Event("Stream read failed.");
                Log.Event("TcpClient-Connection-Closed");
                EndSession();
            }
        }

        private void RecordParsedHandler(object sender, RecordParsedEventArgs e)
        {
            string s = $"{SessionId.ToString()};{e.Record}";
            Parallel.ForEach(_observers, x =>
            {
                x.OnNext(s);
            });
        }

        private void EndSession()
        {
            Disconnected?.Invoke(this, new NewSessionEventArgs(this.SessionId.ToString()));
            _client.Close();
        }

        public IDisposable Subscribe(IAsyncObserver<string> observer)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        [Obsolete("Use async methods", true)]
        public IDisposable Subscribe(IObserver<string> observer)
        {
            throw new NotImplementedException();
        }
    }
}