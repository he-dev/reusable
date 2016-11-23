using System;

namespace Reusable
{
    public static class Conditional
    {
        // ReSharper disable once InconsistentNaming
        public static TResult IIf<TArg, TResult>
        (
            this TArg arg,
            Func<TArg, bool> predicate,
            Func<TArg, TResult> ifTrue,
            Func<TArg, TResult> ifFalse = null
        )
        {
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }
            if (predicate == null) { throw new ArgumentNullException(nameof(ifTrue)); }

            return predicate(arg) ? ifTrue(arg) : (ifFalse == null ? default(TResult) : ifFalse(arg));
        }

        public static bool IfInstanceOf<T>(this object arg, Action<T> body) where T : class
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var argT = arg as T;
            if (argT == null)
            {
                return false;
            }
            body(argT);
            return true;
        }
    }
}
