using System;
using System.Threading;
using CarDataService.Formatters;

namespace CarDataService.Subscribers
{
    public class ConsoleSubscriber : SubscriberBase, IAsyncObserver<string>
    {
        private int _rowsSent = 0;
        private readonly DateTime _startTime = DateTime.UtcNow;

        public ConsoleSubscriber(IRowConverter converter) : base(converter) { }

        public ConsoleSubscriber() { }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            Log.Error(error);
        }

        public void OnNext(string value)
        {
            OnNextAsync(value);
        }

        public void OnNextAsync(string value)
        {
            OnNextAsync(value, CancellationToken.None);
        }

        public void OnNextAsync(string value, CancellationToken token)
        {
            _rowsSent++;
            if (_converter != null)
            {
                Console.WriteLine(_converter.ConvertToStringSimple(value));
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{_rowsSent} rows sent since {_startTime}.");
            }
        }
    }
}