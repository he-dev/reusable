using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Quickey
{
    public static class SelectorExtensions
    {
        public static Selector Index(this Selector selector, string index, string prefix = "[", string suffix = "]")
        {
            if(selector.LastOrDefault() is SelectorIndex) throw new InvalidOperationException($"'{selector.Expression}' already contains an index.");
            return new Selector(selector.Expression, selector.Append(new SelectorName(index) { Prefix = prefix, Suffix = suffix }));
        }
        
        [NotNull, ItemNotNull]
        internal static IEnumerable<SelectorName> GetSelectorNames(this Selector selector)
        {
            var selectorNameEnumerators =
                from m in selector.Members()
                where m.IsDefined(typeof(SelectorNameEnumeratorAttribute))
                select m.GetCustomAttribute<SelectorNameEnumeratorAttribute>();

            // You want to take the nearest one to the member.
            var selectorNameEnumerator = (selectorNameEnumerators.FirstOrDefault() ?? new SelectorNameEnumeratorAttribute());
            return
                selectorNameEnumerator
                    .EnumerateSelectorNames(selector)
                    // You want to take the nearest one to the member.
                    .FirstOrDefault()
                ?? throw new InvalidOperationException($"Either the type '{selector.DeclaringType.ToPrettyString()}' or the member '{selector.Member.Name}' must be decorated with at least one 'UseXAttribute'.");
        }

        /// <summary>
        /// Adds selectors from T to a collection.
        /// </summary>
        public static IImmutableList<Selector> AddFrom<T>(this IImmutableList<Selector> selectors)
        {
            var newSelectors =
                from p in typeof(T).GetProperties()
                select Selector.FromProperty(typeof(T), p);

            return selectors.AddRange(newSelectors);
        }

        /// <summary>
        /// Filters selectors by tag names where T is an attribute that returns a collection of tags.
        /// </summary>
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

        /// <summary>
        /// Formats multiple selectors.
        /// </summary>
        public static IEnumerable<string> Format(this IEnumerable<Selector> selectors)
        {
            return
                from s in selectors
                select s.ToString();
        }

        /// <summary>
        /// Enumerates selector members from last to first.
        /// </summary>
        public static IEnumerable<MemberInfo> Members([NotNull] this Selector selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return selector.Member.AncestorTypesAndSelf();
        }

        private static IEnumerable<MemberInfo> AncestorTypesAndSelf(this MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            var current = member;
            do
            {
                yield return current;

                if (current is Type type)
                {
                    if (type.IsInterface)
                    {
                        yield break;
                    }

                    if (type.GetProperty(member.Name) is PropertyInfo otherProperty)
                    {
                        yield return otherProperty;
                    }
                }

                current = current.DeclaringType;
            } while (!(current is null));
        }
    }
}