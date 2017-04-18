using System.Collections.Generic;
using System.Linq;

namespace Reusable.Fuse
{
    public static class EnumerableValidation
    {
        public static ISpecificationContext<IEnumerable<T>> IsEmpty<T>(this ISpecificationContext<IEnumerable<T>> specificationContext, string message = null)
        {
            return specificationContext.AssertIsFalse(
                value => value.Any(),
                message ?? $"\"{specificationContext.MemberName}\" must be empty.");
        }

        public static ISpecificationContext<IEnumerable<T>> IsNotEmpty<T>(this ISpecificationContext<IEnumerable<T>> specificationContext, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => value.Any(),
                message ?? $"\"{specificationContext.MemberName}\" must not be empty.");
        }

        public static ISpecificationContext<IEnumerable<T>> SequenceEqual<T>(this ISpecificationContext<IEnumerable<T>> specificationContext, IEnumerable<T> other, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => value.SequenceEqual(other),
                message ?? $"Sequence \"{specificationContext.MemberName}\" must contain [{string.Join(", ", other.Quote())}] but does [{string.Join(", ", specificationContext.Value.Quote())}].");
        }

        public static ISpecificationContext<IEnumerable<T>> SequenceEqual<T>(this ISpecificationContext<IEnumerable<T>> specificationContext, IEnumerable<T> other, IEqualityComparer<T> comparer, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => value.SequenceEqual(other, comparer),
                message ?? $"Sequence \"{specificationContext.MemberName}\" must contain [{string.Join(", ", other.Quote())}] but does [{string.Join(", ", specificationContext.Value.Quote())}].");
        }

        public static ISpecificationContext<IDictionary<TKey, TValue>> DictionaryEqual<TKey, TValue>(this ISpecificationContext<IDictionary<TKey, TValue>> specificationContext, IDictionary<TKey, TValue> other, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => value.All(x => other.TryGetValue(x.Key, out TValue v) && x.Value.Equals(v)),
                message ?? $"Sequence \"{specificationContext.MemberName}\" must contain [{string.Join(", ", other.Quote())}] but does [{string.Join(", ", specificationContext.Value.Quote())}].");
        }

        public static ISpecificationContext<IEnumerable<T>> Contains<T>(this ISpecificationContext<IEnumerable<T>> specificationContext, T element, IEqualityComparer<T> comparer = null, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => comparer != null ? value.Contains(element, comparer) : value.Contains(element),
                message ?? $"\"{specificationContext.MemberName}\" collection must contain \"{element}\".");
        }

        public static ISpecificationContext<IEnumerable<T>> DoesNotContain<T>(this ISpecificationContext<IEnumerable<T>> specificationContext, T element, IEqualityComparer<T> comparer = null, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => comparer != null ? !value.Contains(element, comparer) : !value.Contains(element),
                message ?? $"\"{specificationContext.MemberName}\" collection must not contain \"{element}\".");
        }

        private static IEnumerable<string> Quote<T>(this IEnumerable<T> values)
        {
            return values.Select(x => $"\"{x}\"");
        }
    }
}
