using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class Retry : Try
    {
        private readonly GeneratedSequence<TimeSpan> _delaySequence;

        private readonly List<Exception> _exceptions = new List<Exception>();

        public IEnumerable<Exception> Exceptions => _exceptions.AsReadOnly();

        public int Count { get; private set; }

        public bool Success { get; private set; }

        public Retry(GeneratedSequence<TimeSpan> delaySequence)
        {
            _delaySequence = delaySequence;
        }

        public override T Execute<T>(Func<T> body, Action<Attempt> handleException)
        {
            foreach (var delay in _delaySequence)
            {
                var result = default(T);
                if ((Success = TryExecute(body, handleException, out result)))
                {
                    return result;
                }
                Thread.Sleep(delay);
            }
            return default(T);
        }

        public override async Task<T> ExecuteAsync<T>(Func<T> body, CancellationToken cancellationToken, Action<Attempt> handleException)
        {
            foreach (var delay in _delaySequence)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = default(T);
                if ((Success = TryExecute(body, handleException, out result)))
                {
                    return result;
                }
                await Task.Delay(delay, cancellationToken);
            }
            return default(T);
        }

        private bool TryExecute<T>(Func<T> body, Action<Attempt> handleException, out T result)
        {
            result = default(T);

            try
            {
                Count++;
                OnLog($"Attempt #{Count} [Executing...] ({Thread.CurrentThread.ManagedThreadId})");
                result = body();
                OnLog($"Attempt #{Count} [Success] ({Thread.CurrentThread.ManagedThreadId})");
                return true;
            }
            catch (Exception ex)
            {
                OnLog($"Attempt #{Count} [Error] ({Thread.CurrentThread.ManagedThreadId})");
                _exceptions.Add(ex);
                var attempt = new Attempt(ex, Count);
                handleException(attempt);
                if (!attempt.Handled)
                {
                    OnLog($"{ex.GetType().Name} [Throw] ({Thread.CurrentThread.ManagedThreadId})");
                    throw;
                }
                return false;
            }
        }
    }
}
