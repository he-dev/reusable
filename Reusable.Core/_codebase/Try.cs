using System;
using System.Diagnostics;

namespace Reusable
{
    public static class Try
    {
        public static Result Execute(Action action, string message = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
                return Result.Ok(stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return (ex, message, stopwatch.Elapsed);
            }
        }

        public static Result<T> Execute<T>(Func<T> action, string message = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return (action(), stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return (ex, message, stopwatch.Elapsed);
            }
        }
    }
}