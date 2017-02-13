using System;
using System.Diagnostics;

namespace Reusable.Fuse
{
    public static class SpecificationContextExtensions
    {
        public static ISpecificationContext<T> Check<T>(this ISpecificationContext<T> specificationContext, Predicate<T> predicate, string message)
        {
            if (specificationContext == null) throw new ArgumentNullException(nameof(specificationContext));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (string.IsNullOrEmpty(message)) { throw new ArgumentNullException(nameof(message)); }

            if (predicate(specificationContext.Value))
            {
                return specificationContext;
            }

            throw (Exception)Activator.CreateInstance(specificationContext.ExceptionType, message);
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