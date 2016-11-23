using System;
using System.Diagnostics;

namespace Reusable.Fuse
{
    public static class CurrentExtensions
    {
        public static ICurrent<T> Check<T>(this ICurrent<T> current, Predicate<T> predicate, string message)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (string.IsNullOrEmpty(message)) { throw new ArgumentNullException(nameof(message)); }

            if (predicate(current.Value))
            {
                return current;
            }

            throw (Exception)Activator.CreateInstance(current.ExceptionType, message);
        }

        public static ICurrent<T> Throws<T>(this ICurrent<T> current, Type exceptionType)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (exceptionType == null) throw new ArgumentNullException(nameof(exceptionType));
            return new Current<T>(current.Value, current.MemberName, exceptionType);
        }

        [DebuggerStepThrough]
        public static ICurrent<T> Cast<T>(this ICurrent<object> current)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            return new Current<T>((T)current.Value, current.MemberName, current.ExceptionType);
        }

        [DebuggerStepThrough]
        public static ICurrent<TNext> Then<T, TNext>(this ICurrent<T> current, Func<T, TNext> selectNext, string memberName = null)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (selectNext == null) throw new ArgumentNullException(nameof(selectNext));

            return new Current<TNext>(selectNext(current.Value), memberName, current.ExceptionType);
        }
    }
}