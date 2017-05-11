using System;

// ReSharper disable InconsistentNaming

namespace Reusable.Extensions
{
    public static class Conditional
    {
        public static bool IsNull<T>(this T value) => ReferenceEquals(value, null);

        public static bool IsNotNull<T>(this T value) => !IsNull(value);

        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool IsNotNullOrEmpty(this string value) => !IsNullOrEmpty(value);


        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TValue, TResult> ifTrue, Func<TValue, TResult> ifFalse = null)
        {
            return 
                predicate(value)
                    ? ifTrue(value)
                    : ifFalse == null ? default(TResult) : ifFalse(value);
        }

        #region IIf

        // ReSharper disable once InconsistentNaming
        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TResult> ifTrue, Func<TResult> ifFalse = null)
        {
            return value.IIf(
                predicate, 
                x => ifTrue(), 
                x => ifFalse == null ? default(TResult) : ifFalse()
            );
        }

        // ReSharper disable once InconsistentNaming
        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, TResult ifTrue, TResult ifFalse = default(TResult))
        {
            return value.IIf(
                predicate, 
                x => ifTrue, 
                x => ifFalse
            );
        }

        // ReSharper disable once InconsistentNaming
        public static void IIf<TValue>(this TValue arg, Func<TValue, bool> predicate, Action<TValue> ifTrue, Action<TValue> ifFalse = null)
        {
            if (predicate(arg))
            {
                ifTrue(arg);
            }
            else
            {
                ifFalse?.Invoke(arg);
            }
        }

        #endregion

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
