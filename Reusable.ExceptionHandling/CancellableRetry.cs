using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class CancellableRetry : Try
    {
        public CancellableRetry(Retry retry, CircuitBreaker circuitBreaker)
        {
            Retry = retry;
            CircuitBreaker = circuitBreaker;
        }

        private Retry Retry { get; }

        public CircuitBreaker CircuitBreaker { get; }

        public override T Execute<T>(Func<T> body, Action<Attempt> handleException)
        {
            if (CircuitBreaker.State == CircutBreakerState.Open && CircuitBreaker.TimedOut)
            {
                CircuitBreaker.Reset();
                OnLog($"Breaker [Closed] ({Thread.CurrentThread.ManagedThreadId})");
            }

            if (CircuitBreaker.State == CircutBreakerState.Closed)
            {
                try
                {
                    return Retry.Execute(body, attempt => HandleException(attempt, handleException));
                }
                catch (RetryCancelledException)
                {
                    OnLog($"Retry [Cancelled] ({Thread.CurrentThread.ManagedThreadId})");
                    return default(T);
                }
            }

            OnLog($"Breaker [Open] ({Thread.CurrentThread.ManagedThreadId})");
            return default(T);
        }

        public override async Task<T> ExecuteAsync<T>(Func<T> body, CancellationToken cancellationToken, Action<Attempt> handleException)
        {
            if (CircuitBreaker.State == CircutBreakerState.Open && CircuitBreaker.TimedOut)
            {
                CircuitBreaker.Reset();
                OnLog($"Breaker [Closed] ({Thread.CurrentThread.ManagedThreadId})");
            }

            if (CircuitBreaker.State == CircutBreakerState.Open) { return default(T); }

            try
            {
                return await Retry.ExecuteAsync<T>(body, cancellationToken, attempt => HandleException(attempt, handleException));
            }
            catch (RetryCancelledException)
            {
                return default(T);
            }
        }

        private void HandleException(Attempt attempt, Action<Attempt> handleException)
        {
            if (CircuitBreaker.PassOne().State == CircutBreakerState.Open)
            {
                OnLog($"CircuitBreaker [Blown] ({Thread.CurrentThread.ManagedThreadId})");
                throw new RetryCancelledException();
            }

            handleException(attempt);
        }
    }
}
