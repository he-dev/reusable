using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.Data
{
    public static class SelectorExtensions
    {
        [NotNull, ItemNotNull]
        internal static IEnumerable<Key> GetKeys(this Selector selector)
        {
            var keyEnumerators =
                from m in selector.Member.AncestorTypesAndSelf()
                where m.IsDefined(typeof(KeyEnumeratorAttribute))
                select m.GetCustomAttribute<KeyEnumeratorAttribute>();

            var keyEnumerator = (keyEnumerators.FirstOrDefault() ?? new KeyEnumeratorAttribute());
            return
                keyEnumerator
                    .EnumerateKeys(selector)
                    .FirstOrDefault()
                ?? throw new InvalidOperationException($"'{selector}' is not decorated with any keys.");
        }

        public static IImmutableList<Selector> AddFrom<T>(this IImmutableList<Selector> selectors)
        {
            var newSelectors =
                from p in typeof(T).GetProperties()
                select Selector.FromProperty(typeof(T), p);

            return selectors.AddRange(newSelectors);
        }

        public static IEnumerable<Selector> Where<T>(this IEnumerable<Selector> selectors, params string[] names) where T : Attribute, IEnumerable<string>
        {
            if (!names.Any()) throw new ArgumentException("You need to specify at least one tag.");

            return
                from f in selectors
                where f.Tags<T>().IsSubsetOf(names, SoftString.Comparer)
                select f;
        }

        private static IEnumerable<string> Tags<T>(this Selector selector) where T : Attribute, IEnumerable<string>
        {
            var names =
                from m in selector.Member.AncestorTypesAndSelf()
                from ts in m.GetCustomAttributes<T>()
                from t in ts
                select t;

            return names.Distinct(SoftString.Comparer);
        }

        public static IEnumerable<string> Format(this IEnumerable<Selector> selectors)
        {
            return
                from s in selectors
                select s.ToString();
        }
    }
}