using System;

namespace Reusable.Extensions
{
    public static class ConditionalExtensions
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
    }
}
