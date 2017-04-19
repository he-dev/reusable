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

        public static bool Execute(Action action, string message, out Result result)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
                result = Result.Ok(stopwatch.Elapsed);
                return true;
            }
            catch (Exception ex)
            {
                result = Result.Fail(ex, message, stopwatch.Elapsed);
                return false;
            }
        }

        public static bool Execute<T>(Func<T> action, string message, out Result<T> result)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                result = Result<T>.Ok(action(), stopwatch.Elapsed);
                return true;
            }
            catch (Exception ex)
            {
                result = Result<T>.Fail(ex, message, stopwatch.Elapsed);
                return false;
            }
        }

        public static bool Execute<T>(Func<Result<T>> action, string message, out Result<T> result)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                result = Result<T>.Ok(action(), stopwatch.Elapsed);
                return true;
            }
            catch (Exception ex)
            {
                result = Result<T>.Fail(ex, message, stopwatch.Elapsed);
                return false;
            }
        }
    }
}