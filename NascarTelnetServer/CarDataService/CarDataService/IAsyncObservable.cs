using System;

namespace CarDataService
{
    public interface IAsyncObservable<T> : IObservable<T>
    {
        IDisposable Subscribe(IAsyncObserver<T> observer);
    }
}