using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public abstract class Try
    {
        protected static readonly Action<Attempt> EmptyExceptionHandler = attempt => { attempt.Handled = true; };

        public Action<string> Log { get; set; }

        protected void OnLog(string message)
        {
            Log?.Invoke(message);
        }

        public abstract T Execute<T>(Func<T> body, Action<Attempt> handleException);

        public T Execute<T>(Func<T> body)
        {
            return Execute(body, EmptyExceptionHandler);
        }

        public void Execute(Action body, Action<Attempt> handleException)
        {
            Execute<object>(() => { body(); return null; }, handleException);
        }

        public void Execute(Action body)
        {
            Execute<object>(() => { body(); return null; }, EmptyExceptionHandler);
        }

        public abstract Task<T> ExecuteAsync<T>(Func<T> body, CancellationToken cancellationToken, Action<Attempt> handleException);

        public Task<T> ExecuteAsync<T>(Func<T> body)
        {
            return ExecuteAsync(body, CancellationToken.None, EmptyExceptionHandler);
        }

        public Task ExecuteAsync(Action body, Action<Attempt> handleException)
        {
            return ExecuteAsync<object>(() => { body(); return null; }, CancellationToken.None, handleException);
        }

        public Task ExecuteAsync(Action body)
        {
            return ExecuteAsync<object>(() => { body(); return null; }, CancellationToken.None, EmptyExceptionHandler);
        }
    }

    
}
