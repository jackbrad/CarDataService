using System;
using System.Collections.Generic;

namespace CarDataService
{
    internal class Unsubscriber : IDisposable
    {
        private readonly List<IAsyncObserver<string>> _observers;
        private readonly IAsyncObserver<string> _observer;

        public Unsubscriber(List<IAsyncObserver<string>> observers, IAsyncObserver<string> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer);
        }

        public void Complete()
        {
            _observer.OnCompleted();
        }
    }
}