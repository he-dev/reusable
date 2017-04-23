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

        public static void Execute<TLocal>(Action action, Func<TLocal> preAction, Action<TLocal> postAction)
        {
            var local = preAction();
            try
            {
                action();
                postAction(local);
            }
            catch (Exception)
            {
                postAction(local);
                throw;
            }
        }

        public static TResult Execute<TLocal, TResult>(Func<TResult> action, Func<TLocal> preAction, Action<TLocal> postAction)
        {
            var local = preAction();
            try
            {
                var result = action();
                postAction(local);
                return result;
            }
            catch (Exception)
            {
                postAction(local);
                throw;
            }
        }
    }
}