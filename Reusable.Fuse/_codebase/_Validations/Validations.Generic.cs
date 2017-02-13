using System;

namespace Reusable.Fuse
{
    public static class GenericValidation
    {
        //private static readonly IFormatProvider FormatProvider = new QuoteFormatter();

        public static ISpecificationContext<T> IsNull<T>(this ISpecificationContext<T> specificationContext, string message = null)
        {
            return specificationContext.Check(
                value => ReferenceEquals(value, null),
                message ?? $"\"{specificationContext.MemberName}\" must be null.");
        }

        public static ISpecificationContext<T> IsNotNull<T>(this ISpecificationContext<T> specificationContext, string message = null)
        {
            return specificationContext.Check(
                value => !ReferenceEquals(value, null),
                message ?? $"\"{specificationContext.MemberName}\" must not be null.");
        }
        
        public static ISpecificationContext<T> IsLessThen<T>(this ISpecificationContext<T> specificationContext, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(max) < 0,
                message ?? $"\"{specificationContext.MemberName}\" must be less then \"{max}\".");
        }

        public static ISpecificationContext<T> IsLessThenOrEqual<T>(this ISpecificationContext<T> specificationContext, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(max) <= 0,
                message ?? $"\"{specificationContext.MemberName}\" must be less then or equal \"{max}\".");
        }

        public static ISpecificationContext<T> IsGreaterThen<T>(this ISpecificationContext<T> specificationContext, T min, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(min) > 0,
                message ?? $"\"{specificationContext.MemberName}\" must be greater then \"{min}\".");
        }

        public static ISpecificationContext<T> IsGreaterThenOrEqual<T>(this ISpecificationContext<T> specificationContext, T min, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(min) >= 0,
                message ?? $"\"{specificationContext.MemberName}\" must be greater or equal \"{min}\".");
        }

        public static ISpecificationContext<T> IsBetween<T>(this ISpecificationContext<T> specificationContext, T min, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(min) > 0 && value.CompareTo(max) < 0,
                message ?? $"\"{specificationContext.MemberName}\" must be between \"{min}\" and \"{max}\".");
        }

        public static ISpecificationContext<T> IsBetweenOrEqual<T>(this ISpecificationContext<T> specificationContext, T min, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
                message ?? $"\"{specificationContext.MemberName}\" must be between or equal \"{min}\" and \"{max}\".");
        }

        public static ISpecificationContext<T> IsEqual<T>(this ISpecificationContext<T> specificationContext, T equalValue, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(equalValue) == 0,
                message ?? $"\"{specificationContext.MemberName}\" must be equal \"{equalValue}\" but is \"{specificationContext.Value}\".");
        }

        public static ISpecificationContext<T> IsNotEqual<T>(this ISpecificationContext<T> specificationContext, T notEqualValue, string message = null)
            where T : IComparable
        {
            return specificationContext.Check(
                value => value.CompareTo(notEqualValue) != 0,
                message ?? $"\"{specificationContext.MemberName}\" must not be equal \"{notEqualValue}\".");
        }
        
        public static ISpecificationContext<T> IsTrue<T>(this ISpecificationContext<T> specificationContext, Func<T, bool> predicate, string message = null)
        {
            return specificationContext.Check(
                value => predicate(value),
                message ?? $"\"{specificationContext.MemberName}\" must be \"{bool.TrueString}\".");
        }

        public static ISpecificationContext<T> IsFalse<T>(this ISpecificationContext<T> specificationContext, Func<T, bool> predicate, string message = null)
        {
            return specificationContext.Check(
                value => !predicate(value),
                message ?? $"\"{specificationContext.MemberName}\" must be \"{bool.FalseString}\".");
        }

        public static ISpecificationContext<T> IsInstanceOfType<T>(this ISpecificationContext<T> specificationContext, Type type, string message = null)
        {
            return specificationContext.Check(
                value => type.IsInstanceOfType(value),
                message ?? $"\"{specificationContext.MemberName}\" must be instance of \"{type.FullName}\".");
        }
        
        public static ISpecificationContext<Type> IsAssignableTo<T>(this ISpecificationContext<Type> specificationContext, string message = null)
        {
            return specificationContext.Check(
                value => typeof(T).IsAssignableFrom(value),
                message ?? $"\"{specificationContext.MemberName}\" must be assignable from \"{typeof(T).FullName}\".");
        }
    }    
}
