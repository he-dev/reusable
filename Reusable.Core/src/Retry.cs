using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable
{
    public interface IRetry : IObservable<Exception>
    {
        Task<T> ExecuteAsync<T>([NotNull] Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
    }

    public class Retry : IRetry
    {
        private readonly IEnumerable<TimeSpan> _delays;

        private readonly ISet<IObserver<Exception>> _observers;

        public Retry([NotNull] IEnumerable<TimeSpan> delays)
        {
            // Deliberately not materializing the delays. We don't know whether this is not infinite.
            _delays = delays ?? throw new ArgumentNullException(nameof(delays));
            _observers = new HashSet<IObserver<Exception>>();
        }

        public IDisposable Subscribe(IObserver<Exception> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            if (_observers.Add(observer))
            {
                return Disposable.Create(() =>
                {
                    _observers.Remove(observer);
                });
            }
            else
            {
                return Disposable.Empty;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();

            foreach (var delay in _delays)
            {
                ThrowIfCancellationRequested(cancellationToken);

                try
                {
                    return await action(cancellationToken);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);

                    foreach (var observer in _observers)
                    {
                        observer.OnNext(ex);
                    }

                    // Don't delay if cancelled but return immediately.
                    ThrowIfCancellationRequested(cancellationToken);

                    await Task.Delay(delay, cancellationToken);
                }
            }

            throw new AggregateException($"Action failed after {exceptions.Count} attempt(s).", exceptions);
        }

        ///// <summary>
        ///// Utility method that creates a Retry and calls the ExecuteAsync method.
        ///// </summary>
        ///// <returns></returns>
        //public async Task<T> ExecuteAsync<T>(IEnumerable<TimeSpan> delays, Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
        //{
        //    return await new Retry(delays).ExecuteAsync(action, cancellationToken);
        //}

        private void ThrowIfCancellationRequested(CancellationToken cancellationToken)
        {
            foreach (var retryBreaker in _observers.OfType<IRetryBreaker>())
            {
                retryBreaker.Token.ThrowIfCancellationRequested();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    internal interface IRetryBreaker : IObserver<Exception>, IDisposable
    {
        CancellationToken Token { get; }
    }

    // todo: not tested yet
    //internal class RetryBreaker : IRetryBreaker
    //{
    //    private readonly IObserver<Exception> _observer;

    //    private CircutBrakerState _state = CircutBrakerState.Closed;

    //    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    //    private RetryBreaker(int maxFailureCount, TimeSpan maxFailureInterval, Func<Exception, bool> filter = null)
    //    {
    //        //var openFuse = Observer.Create<Exception>(exception =>
    //        //{
    //        //    switch (_state)
    //        //    {
    //        //        case CircutBrakerState.Closed:
    //        //            _state = CircutBrakerState.Open;
    //        //            _cancellationTokenSource.Cancel();
    //        //            //Debug.WriteLine($"Circut broken! [{Thread.CurrentThread.ManagedThreadId}]");
    //        //            break;

    //        //        case CircutBrakerState.HalfOpen:
    //        //            // I currently don't have any use case for this.
    //        //            break;

    //        //        case CircutBrakerState.Open:
    //        //            // There is nothing to do when Open.
    //        //            break;
    //        //    }
    //        //});

    //        //_observer = new Subject<Exception>();
    //        //((IObservable<Exception>)_observer)
    //        //    .Where(ex => filter == null || filter(ex))
    //        //    .Fuse(maxFailureCount, maxFailureInterval)
    //        //    .Subscribe(openFuse);
    //    }

    //    public CircutBrakerState State => _state;

    //    public CancellationToken Token => _cancellationTokenSource.Token;

    //    public void OnNext(Exception exception) => _observer.OnNext(exception);

    //    public void OnCompleted() => _observer.OnCompleted();

    //    public void OnError(Exception exception) => _observer.OnError(exception);

    //    public void Reset()
    //    {
    //        _state = CircutBrakerState.Closed;
    //        _cancellationTokenSource.Dispose();
    //        _cancellationTokenSource = new CancellationTokenSource();
    //    }

    //    public void Dispose() => _cancellationTokenSource.Dispose();

    //    public static RetryBreaker Create(int maxFailureCount, TimeSpan maxFailureInterval, Func<Exception, bool> filter = null)
    //    {
    //        return new RetryBreaker(maxFailureCount, maxFailureInterval, filter);
    //    }
    //}

    //public enum CircutBrakerState
    //{
    //    Closed,
    //    HalfOpen,
    //    Open,
    //}
}
