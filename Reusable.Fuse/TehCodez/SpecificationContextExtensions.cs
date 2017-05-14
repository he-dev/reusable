using System;
using System.Diagnostics;

namespace Reusable.Fuse
{
    public static class SpecificationContextExtensions
    {
        public static ISpecificationContext<T> AssertIsTrue<T>(this ISpecificationContext<T> context, Predicate<T> predicate, string message)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (string.IsNullOrEmpty(message)) { throw new ArgumentNullException(nameof(message)); }

            return
                predicate(context.Value) == true
                    ? context
                    : throw (Exception)Activator.CreateInstance(context.ExceptionType, message);
        }

        public static ISpecificationContext<T> AssertIsFalse<T>(this ISpecificationContext<T> context, Predicate<T> predicate, string message)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (string.IsNullOrEmpty(message)) { throw new ArgumentNullException(nameof(message)); }

            return
                predicate(context.Value) == false
                    ? context
                    : throw (Exception)Activator.CreateInstance(context.ExceptionType, message);
        }

        public static ISpecificationContext<T> Throws<T>(this ISpecificationContext<T> specificationContext, Type exceptionType)
        {
            if (specificationContext == null) throw new ArgumentNullException(nameof(specificationContext));
            if (exceptionType == null) throw new ArgumentNullException(nameof(exceptionType));
            return new SpecificationContext<T>(specificationContext.Value, specificationContext.MemberName, exceptionType);
        }

        [DebuggerStepThrough]
        public static ISpecificationContext<T> Cast<T>(this ISpecificationContext<object> specificationContext)
        {
            if (specificationContext == null) throw new ArgumentNullException(nameof(specificationContext));
            return new SpecificationContext<T>((T)specificationContext.Value, specificationContext.MemberName, specificationContext.ExceptionType);
        }

        [DebuggerStepThrough]
        public static ISpecificationContext<TNext> Then<T, TNext>(this ISpecificationContext<T> specificationContext, Func<T, TNext> selectNext, string memberName = null)
        {
            if (specificationContext == null) throw new ArgumentNullException(nameof(specificationContext));
            if (selectNext == null) throw new ArgumentNullException(nameof(selectNext));

            return new SpecificationContext<TNext>(selectNext(specificationContext.Value), memberName, specificationContext.ExceptionType);
        }
    }
}