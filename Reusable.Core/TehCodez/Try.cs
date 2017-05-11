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

        public static bool Execute<TLocal, TResult>(Func<TResult> action, Func<TLocal> preAction, Action<TLocal, Exception> postAction, out TResult result)
        {
            var local = preAction();
            try
            {
                result = action();
                postAction(local, null);
                return true;
            }
            catch (Exception ex)
            {
                result = default(TResult);
                postAction(local, ex);
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
            catch (Exception)
            {
                postAction(local, false);
                throw;
            }
        }

        public static TResult Execute<TLocal, TResult>(Func<TResult> action, Func<TLocal> preAction, Action<TLocal, bool> postAction)
        {
            var local = preAction();
            try
            {
                var result = action();
                postAction(local, true);
                return result;
            }
            catch (Exception)
            {
                postAction(local, false);
                throw;
            }
        }
    }
}