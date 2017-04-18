using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Reusable.Fuse
{
    public static class GenericValidation
    {
        //private static readonly IFormatProvider FormatProvider = new QuoteFormatter();

        public static ISpecificationContext<T> IsNull<T>(this ISpecificationContext<T> specificationContext, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => ReferenceEquals(value, null),
                message ?? $"\"{specificationContext.MemberName}\" must be null.");
        }

        public static ISpecificationContext<T> IsNotNull<T>(this ISpecificationContext<T> specificationContext, string message = null)
        {
            return specificationContext.AssertIsFalse(
                value => ReferenceEquals(value, null),
                message ?? $"\"{specificationContext.MemberName}\" must not be null.");
        }

        public static ISpecificationContext<T> IsLessThen<T>(this ISpecificationContext<T> specificationContext, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(max) < 0,
                message ?? $"\"{specificationContext.MemberName}\" must be less then \"{max}\".");
        }

        public static ISpecificationContext<T> IsLessThenOrEqual<T>(this ISpecificationContext<T> specificationContext, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(max) <= 0,
                message ?? $"\"{specificationContext.MemberName}\" must be less then or equal \"{max}\".");
        }

        public static ISpecificationContext<T> IsGreaterThen<T>(this ISpecificationContext<T> specificationContext, T min, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(min) > 0,
                message ?? $"\"{specificationContext.MemberName}\" must be greater then \"{min}\".");
        }

        public static ISpecificationContext<T> IsGreaterThenOrEqual<T>(this ISpecificationContext<T> specificationContext, T min, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(min) >= 0,
                message ?? $"\"{specificationContext.MemberName}\" must be greater or equal \"{min}\".");
        }

        public static ISpecificationContext<T> IsBetween<T>(this ISpecificationContext<T> specificationContext, T min, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(min) > 0 && value.CompareTo(max) < 0,
                message ?? $"\"{specificationContext.MemberName}\" must be between \"{min}\" and \"{max}\".");
        }

        public static ISpecificationContext<T> IsBetweenOrEqual<T>(this ISpecificationContext<T> specificationContext, T min, T max, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
                message ?? $"\"{specificationContext.MemberName}\" must be between or equal \"{min}\" and \"{max}\".");
        }

        public static ISpecificationContext<T> IsEqual<T>(this ISpecificationContext<T> specificationContext, T equalValue, string message = null)
            where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(equalValue) == 0,
                message ?? $"\"{specificationContext.MemberName}\" must be equal \"{equalValue}\" but is \"{specificationContext.Value}\".");
        }

        public static ISpecificationContext<T> IsEqual<T>(this ISpecificationContext<T> context, T other, IEqualityComparer<T> comparer, string message = null)
        {
            return context.AssertIsTrue(value => comparer.Equals(value, other), message ?? $"\"{context.MemberName}\" must be equal \"{other}\" but is \"{context.Value}\".");
        }

        public static ISpecificationContext<T> IsNotEqual<T>(this ISpecificationContext<T> specificationContext, T notEqualValue, string message = null) where T : IComparable
        {
            return specificationContext.AssertIsTrue(
                value => value.CompareTo(notEqualValue) != 0,
                message ?? $"\"{specificationContext.MemberName}\" must not be equal \"{notEqualValue}\".");
        }

        public static ISpecificationContext<T> IsTrue<T>(this ISpecificationContext<T> specificationContext, Func<T, bool> predicate, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => predicate(value),
                message ?? $"\"{specificationContext.MemberName}\" must be \"{bool.TrueString}\".");
        }

        public static ISpecificationContext<T> IsFalse<T>(this ISpecificationContext<T> specificationContext, Func<T, bool> predicate, string message = null)
        {
            return specificationContext.AssertIsFalse(
                value => predicate(value),
                message ?? $"\"{specificationContext.MemberName}\" must be \"{bool.FalseString}\".");
        }

        public static ISpecificationContext<T> IsInstanceOfType<T>(this ISpecificationContext<T> specificationContext, Type type, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => type.IsInstanceOfType(value),
                message ?? $"\"{specificationContext.MemberName}\" must be instance of \"{type.FullName}\".");
        }

        public static ISpecificationContext<Type> IsAssignableTo<T>(this ISpecificationContext<Type> specificationContext, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => typeof(T).IsAssignableFrom(value),
                message ?? $"\"{specificationContext.MemberName}\" must be assignable from \"{typeof(T).FullName}\".");
        }

    }
}

// System.Drawing types need to be in a separate namespace because otherwise they pollute everything and the reference to this namespace needs to be added.
namespace Reusable.Fuse.Drawing
{
    public static class DrawingValidations
    {
        public static ISpecificationContext<Color> IsEqual(this ISpecificationContext<Color> specificationContext, Color equalValue, string message = null)
        {
            return specificationContext.AssertIsTrue(
                // Default color equality does not work because it compares the name too which may differ even though the colors are the same.
                value => value.ToArgb() == equalValue.ToArgb(),
                message ?? $"\"{specificationContext.MemberName}\" must be equal \"{equalValue}\" but is \"{specificationContext.Value}\".");
        }

        public static ISpecificationContext<Color> IsNotEqual(this ISpecificationContext<Color> specificationContext, Color equalValue, string message = null)
        {
            return specificationContext.AssertIsFalse(
                value => value == equalValue,
                message ?? $"\"{specificationContext.MemberName}\" must be equal \"{equalValue}\" but is \"{specificationContext.Value}\".");
        }
    }
}
