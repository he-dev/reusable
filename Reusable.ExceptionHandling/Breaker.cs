using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class Breaker : Try
    {
        public Breaker(Retry retry, Fuse fuse)
        {
            Retry = retry;
            Fuse = fuse;
        }

        private Retry Retry { get; }

        public Fuse Fuse { get; }

        public override T Execute<T>(Func<T> body, Action<Attempt> handleException)
        {
            if (Fuse.Blown && Fuse.TimedOut)
            {
                Fuse.Reset();
                OnLog($"Breaker [Closed] ({Thread.CurrentThread.ManagedThreadId})");
            }

            if (!Fuse.Blown)
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
            if (Fuse.Blown && Fuse.TimedOut)
            {
                Fuse.Reset();
                OnLog($"Breaker [Closed] ({Thread.CurrentThread.ManagedThreadId})");
            }

            if (Fuse.Blown) { return default(T); }

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
            if (Fuse.PassOne().Blown)
            {
                OnLog($"Fuse [Blown] ({Thread.CurrentThread.ManagedThreadId})");
                throw new RetryCancelledException();
            }

            handleException(attempt);
        }
    }
}
