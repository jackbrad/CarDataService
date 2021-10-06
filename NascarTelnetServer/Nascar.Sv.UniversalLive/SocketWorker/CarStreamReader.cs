using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace Nascar.Sv.UniversalLive.SocketWorker
{
    internal class CarStreamReader : IObservable<string>
    {
        public event EventHandler Connected;

        public CarStreamReader() : base()
        {
            _observers = new List<IObserver<string>>();
        }

        private readonly List<IObserver<string>> _observers;

        private StreamSocket _client;

        private async Task<bool> Connect(string server, int port)
        {
            try
            {
                _client = new StreamSocket();
                await _client.ConnectAsync(new HostName(server), port.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //var stream = s.InputStream.AsStreamForRead();
        //var reader = new StreamReader(stream);
        //var buff = new byte[256];
        //            while (!reader.EndOfStream)
        //            {
        //                var bytes = stream.Read(buff, 0, buff.Length);
        //                if (bytes == 0) { }
        //                Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
        //                {
        //                    var p = new Windows.UI.Xaml.Controls.Maps.MapIcon();
        //p.Location = new Geopoint(new BasicGeoposition() { });
        //                    Data.Add(Encoding.ASCII.GetString(buff));
        //                });
        //            }

        public async void Start(string server, int port)
        {
            do
            {
                var connected = await Connect(server, port);

                if (connected)
                {
                    Connected?.Invoke(this, new EventArgs());
                    //when failed ... reconnect
                    await ReadStream();
                }
                await Task.Delay(10000);
            } while (true);
        }

        private async Task ReadStream()
        {
            var data = new byte[256];
            var sbuffer = new StringBuilder(256);
            var mb = new MessageBuffer();

            try
            {
                var s = _client.InputStream.AsStreamForRead();
                while (true)
                {
                    //var stream = _client.GetStream();
                    //if (!stream.DataAvailable)
                    //{
                    //    //If data is not available in roughly 15 seconds.. disconnect
                    //    if (contCount > 15)
                    //    {
                    //        _client.Close();
                    //        return;
                    //    }

                    //    Task.Delay(1000);
                    //    contCount++;
                    //    continue;
                    //}

                    var bytes = await s.ReadAsync(data, 0, data.Length);
                    sbuffer.Clear();
                    sbuffer.Append(Encoding.ASCII.GetString(data, 0, bytes));

                    mb.AddData(sbuffer.ToString()).ForEach(m =>
                    {
                        sbuffer.Clear().Append(m.Trim());
                        _observers.ForEach(x => x.OnNext(sbuffer.ToString()));
                    }
                );
                }

            }
            catch (Exception e)
            {
                _client.Dispose();
            }
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<string>> _observers;
            private readonly IObserver<string> _observer;

            public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer);
            }
        }
    }
}
