using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.Validations
{
    public static class EnumerableValidation
    {
        public static IValidationContext<IEnumerable<T>> IsEmpty<T>(this IValidationContext<IEnumerable<T>> context, string message = null)
        {
            return context.Validate(
                value => !value.Any(),
                message ?? "{MemberName:dq} must be empty.".Format(new { context.MemberName }));
        }

        public static IValidationContext<IEnumerable<T>> IsNotEmpty<T>(this IValidationContext<IEnumerable<T>> context, string message = null)
        {
            return context.Validate(
                value => value.Any(),
                message ?? "{MemberName:dq} must not be empty.".Format(new { context.MemberName }));
        }

        public static IValidationContext<IEnumerable<T>> SequenceEqual<T>(this IValidationContext<IEnumerable<T>> context, IEnumerable<T> other, string message = null)
        {
            return context.Validate(
                value => value.SequenceEqual(other),
                message ?? "{MemberName:dq} must have the same elements as the other collection.".Format(new { context.MemberName }));
        }

        public static IValidationContext<IEnumerable<T>> Contains<T>(this IValidationContext<IEnumerable<T>> context, T element, IEqualityComparer<T> comparer = null , string message = null)
        {
            return context.Validate(
                value => comparer != null ? value.Contains(element, comparer) : value.Contains(element),
                message ?? "{MemberName:dq} collection must contain {element:dq}.".Format(new { context.MemberName }));
        }

        public static IValidationContext<IEnumerable<T>> DoesNotContain<T>(this IValidationContext<IEnumerable<T>> context, T element, IEqualityComparer<T> comparer = null , string message = null)
        {
            return context.Validate(
                value => comparer != null ? !value.Contains(element, comparer) : !value.Contains(element),
                message ?? "{MemberName:dq} collection must not contain {element:dq}.".Format(new { context.MemberName }));
        }
    }
}
