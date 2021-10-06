using System;
using System.Threading;

namespace CarDataService
{
    public interface IAsyncObserver<T> : IObserver<T>
    {
        void OnNextAsync(T value);

        void OnNextAsync(T value, CancellationToken token);
    }
}