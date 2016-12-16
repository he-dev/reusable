using System.Collections.Generic;
using System.Linq;

namespace Reusable.Fuse
{
    public static class EnumerableValidation
    {
        public static ICurrent<IEnumerable<T>> IsEmpty<T>(this ICurrent<IEnumerable<T>> current, string message = null)
        {
            return current.Check(
                value => !value.Any(),
                message ?? $"\"{current.MemberName}\" must be empty.");
        }

        public static ICurrent<IEnumerable<T>> IsNotEmpty<T>(this ICurrent<IEnumerable<T>> current, string message = null)
        {
            return current.Check(
                value => value.Any(),
                message ?? $"\"{current.MemberName}\" must not be empty.");
        }

        public static ICurrent<IEnumerable<T>> SequenceEqual<T>(this ICurrent<IEnumerable<T>> current, IEnumerable<T> other, string message = null)
        {
            return current.Check(
                value => value.SequenceEqual(other),
                message ?? $"Sequence \"{current.MemberName}\" must contain [{string.Join(", ", other.Quote())}] but does [{string.Join(", ", current.Value.Quote())}].");
        }

        public static ICurrent<IEnumerable<T>> Contains<T>(this ICurrent<IEnumerable<T>> current, T element, IEqualityComparer<T> comparer = null , string message = null)
        {
            return current.Check(
                value => comparer != null ? value.Contains(element, comparer) : value.Contains(element),
                message ?? $"\"{current.MemberName}\" collection must contain \"{element}\".");
        }

        public static ICurrent<IEnumerable<T>> DoesNotContain<T>(this ICurrent<IEnumerable<T>> current, T element, IEqualityComparer<T> comparer = null , string message = null)
        {
            return current.Check(
                value => comparer != null ? !value.Contains(element, comparer) : !value.Contains(element),
                message ?? $"\"{current.MemberName}\" collection must not contain \"{element}\".");
        }

        private static IEnumerable<string> Quote<T>(this IEnumerable<T> values)
        {
            return values.Select(x => $"\"{x}\"");
        }
    }
}
