using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Reusable.Fuse.Testing
{
    public static class VerificationBuilder
    {
        [DebuggerStepThrough]
        public static ISpecificationContext<T> Verify<T>(this T value, string memberName)
        {
            return new SpecificationContext<T>(value, memberName, typeof(VerificationException));
        }

        [DebuggerStepThrough]
        public static ISpecificationContext<T> Verify<T>(this T value)
        {
            return Verify(value, nameof(value));
        }

        [DebuggerStepThrough]
        public static ISpecificationContext<IEnumerable<T>> Verify<T>(this IEnumerable<T> value, string memberName)
        {
            return new SpecificationContext<IEnumerable<T>>(value, memberName, typeof(VerificationException));
        }

        [DebuggerStepThrough]
        public static ISpecificationContext<IEnumerable<T>> Verify<T>(this IEnumerable<T> value)
        {
            return Verify(value, nameof(value));
        }
    }
}