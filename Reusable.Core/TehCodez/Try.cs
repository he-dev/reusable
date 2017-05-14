using System;
using System.Diagnostics;

namespace Reusable
{
    public static class Try
    {
        public static bool Execute<TLocal>(Action action, Func<TLocal> preAction, Action<TLocal, Exception> postAction)
        {
            var local = preAction();
            try
            {
                action();
                postAction(local, null);
                return true;
            }
            catch (Exception ex)
            {
                postAction(local, ex);
                return false;
            }
        }

        public static bool Execute<TLocal, TResult>(Func<TResult> action, Func<TLocal> preAction, Action<TLocal, TResult, Exception> postAction, out TResult result)
        {
            var local = preAction();
            try
            {
                result = action();
                postAction(local, result, default(Exception));
                return true;
            }
            catch (Exception ex)
            {
                result = default(TResult);
                postAction(local, result, ex);
                return false;
            }
        }

        public static void Execute<TLocal>(Action action, Func<TLocal> preAction, Action<TLocal, bool> postAction)
        {
            var local = preAction();
            try
            {
                action();
                postAction(local, true);
            }
            catch
            {
                postAction(local, false);
                throw;
            }
        }

        public static TResult Execute<TLocal, TResult>(Func<TResult> action, Func<TLocal> preAction, Action<TLocal, TResult, bool> postAction)
        {
            var local = preAction();
            try
            {
                var result = action();
                postAction(local, result, true);
                return result;
            }
            catch
            {
                postAction(local, default(TResult), false);
                throw;
            }
        }
    }
}