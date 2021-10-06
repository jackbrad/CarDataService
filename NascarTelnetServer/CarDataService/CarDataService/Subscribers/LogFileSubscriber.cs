using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CarDataService.Subscribers
{
    public class LogFileSubscriber : SubscriberBase, IAsyncObserver<string>
    {
        private readonly string _path;

        public LogFileSubscriber(string path)
        {
            _path = CleanPath(path);
            Log.Event($"{GetType().Name}::Local Logging Started", new Dictionary<string, string>() { { "FilePath", _path }, { "OriginalPath", _path } });
        }

        private static string CleanPath(string path)
        {
            if (path.IndexOf('\\') == -1) //relative, add to base dir
            {
                path = $"{AppDomain.CurrentDomain.BaseDirectory}{path}";
            }

            var directoryPath = Path.GetDirectoryName(path);
            var directoryInfo = Directory.CreateDirectory(directoryPath);
            var p = Path.GetFileName(path);
            var fullPath = $@"{directoryInfo.FullName}\{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}-{p}";
            return fullPath;
        }

        public void OnNext(string value)
        {
            //Task.Run(() => { OnNextAsync(value); });
            File.AppendAllText(_path, value + "\r\n");
        }

        public void OnNextAsync(string value)
        {
            OnNextAsync(value, CancellationToken.None);
        }

        public async void OnNextAsync(string value, CancellationToken token)
        {
            //todo: handle cancellation token
            //File.AppendAllText(_path, value + "\r\n");
        }

        //public void OnNext(string value)
        //{
        //    File.AppendAllText(_path, value + "\r\n");
        //}

        public void OnCompleted()
        {
            Log.Info("LogFileSubscriber completed");
        }

        public void OnError(Exception error)
        {
            Log.Error(error);
        }
    }
}