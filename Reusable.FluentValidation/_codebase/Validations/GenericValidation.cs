using System;
using Reusable.Extensions;

namespace Reusable.FluentValidation.Validations
{
    public static class GenericValidation
    {
        public static IValidationContext<T> IsNull<T>(this IValidationContext<T> context, string message = null)
        {
            return context.Validate(
                value => ReferenceEquals(value, null),
                message ?? "{MemberName:dq} must be null.".Format(new { context.MemberName }));
        }

        public static IValidationContext<T> IsNotNull<T>(this IValidationContext<T> context, string message = null)
        {
            return context.Validate(
                value => !ReferenceEquals(value, null),
                message ?? "{MemberName:dq} must not be null.".Format(new { context.MemberName }));
        }
        
        public static IValidationContext<T> IsLessThen<T>(this IValidationContext<T> context, T max, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(max) < 0,
                message ?? "{MemberName:dq} must be less then {max:dq}.".Format(new { context.MemberName, max }));
        }

        public static IValidationContext<T> IsLessThenOrEqual<T>(this IValidationContext<T> context, T max, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(max) <= 0,
                message ?? "{MemberName:dq} must be less then or equal {max:dq}.".Format(new { context.MemberName, max }));
        }

        public static IValidationContext<T> IsGreaterThen<T>(this IValidationContext<T> context, T min, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(min) > 0,
                message ?? "{MemberName:dq} must be greater then {min:dq}.".Format(new { context.MemberName, min }));
        }

        public static IValidationContext<T> IsGreaterThenOrEqual<T>(this IValidationContext<T> context, T min, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(min) >= 0,
                message ?? "{MemberName:dq} must be greater or equal {min:dq.}".Format(new { context.MemberName, min }));
        }

        public static IValidationContext<T> IsBetween<T>(this IValidationContext<T> context, T min, T max, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(min) > 0 && value.CompareTo(max) < 0,
                message ?? "{MemberName:dq}' must be between {min:dq} and {max:dq}.".Format(new { context.MemberName, min, max }));
        }

        public static IValidationContext<T> IsBetweenOrEqual<T>(this IValidationContext<T> context, T min, T max, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
                message ?? "{MemberName:dq}' must be between or equal {min:dq} and {max:dq}.".Format(new { context.MemberName, min, max }));
        }

        public static IValidationContext<T> IsEqual<T>(this IValidationContext<T> context, T equalValue, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(equalValue) == 0,
                message ?? "{MemberName:dq} must be equal {equalValue:dq} but is {Value:dq}.".Format(new { context.MemberName, equalValue, context.Value }));
        }

        public static IValidationContext<T> IsNotEqual<T>(this IValidationContext<T> context, T notEqualValue, string message = null)
            where T : IComparable
        {
            return context.Validate(
                value => value.CompareTo(notEqualValue) != 0,
                message ?? "{MemberName:dq} must not be equal {notEqualValue:dq}.".Format(new { context.MemberName, notEqualValue }));
        }
        
        public static IValidationContext<T> IsTrue<T>(this IValidationContext<T> context, Func<T, bool> predicate, string message = null)
        {
            return context.Validate(
                value => predicate(value),
                message ?? "{MemberName:dq} must be {TrueString:dq}.".Format(new { context.MemberName, bool.TrueString }));
        }

        public static IValidationContext<T> IsFalse<T>(this IValidationContext<T> context, Func<T, bool> predicate, string message = null)
        {
            return context.Validate(
                value => !predicate(value),
                message ?? "{MemberName:dq} must be {FalseString:dq}.".Format(new { context.MemberName, bool.FalseString }));
        }

        public static IValidationContext<T> IsInstanceOfType<T>(this IValidationContext<T> context, Type type, string message = null)
        {
            return context.Validate(
                value => type.IsInstanceOfType(value),
                message ?? "{MemberName:dq} must be instance of {FullName:dq}.".Format(new { context.MemberName, type.FullName }));
        }
    }    
}
