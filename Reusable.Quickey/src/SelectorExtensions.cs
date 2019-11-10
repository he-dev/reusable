using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Quickey
{
    public static class SelectorExtensions
    {
        public static Selector Index(this Selector selector, string index)
        {
            var nearestTokenProvider = selector.Member.TokenProviders().First();
            var indexToken =
                nearestTokenProvider
                    .GetSelectorTokenFactories<IVariableSelectorTokenFactory>(selector.Member)
                    .First()
                    .Where(stf => stf.TokenType == SelectorTokenType.Index)
                    .SingleOrThrow(onEmpty: () => DynamicException.Create($"{nameof(UseIndexAttribute)}NotFound", $"'{selector}' must be decorated with {nameof(UseIndexAttribute)}."));

            return new Selector(selector.Expression, selector.Append(indexToken.CreateSelectorToken(selector.Member, index)));
        }

        /// <summary>
        /// Adds selectors from T to a collection.
        /// </summary>
        public static IImmutableList<Selector> AddFrom<T>(this IImmutableList<Selector> selectors)
        {
            var newSelectors =
                from p in typeof(T).GetProperties()
                select
                    typeof(Selector).IsAssignableFrom(p.PropertyType)
                        ? (Selector)p.GetValue(default)
                        : (Selector)Selector.FromMember(typeof(T), p);

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
                where f.Tags<T>().IsSupersetOf(names, SoftString.Comparer)
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
        /// Enumerates selector path in the class hierarchy from member to parent.
        /// </summary>
        public static IEnumerable<MemberInfo> Path([NotNull] this MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return member.AncestorTypesAndSelf();
        }

        private static IEnumerable<MemberInfo> AncestorTypesAndSelf(this MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            var current = member;
            do
            {
                yield return current;

                switch (current)
                {
                    case FieldInfo field:
                        current = field.ReflectedType;
                        break;

                    case PropertyInfo property:
                        current = property.ReflectedType;
                        break;

                    case Type type:
                        if (type.IsInterface)
                        {
                            yield break;
                        }

                        type = type.BaseType;

                        if (type is null || type == typeof(object))
                        {
                            yield break;
                        }

                        if (type.GetProperty(member.Name) is PropertyInfo otherProperty)
                        {
                            yield return otherProperty;
                        }

                        current = type;
                        break;
                }
            } while (!(current is null));
        }

        public static StringSelector<T> AsString<T>(this Selector<T> selector) => new StringSelector<T>(selector);

        [NotNull, ItemNotNull]
        public static IEnumerable<ISelectorTokenProvider> TokenProviders(this MemberInfo member)
        {
            var tokenProviders =
                from m in member.Path()
                where m.IsDefined(typeof(SelectorTokenProviderAttribute))
                select m.GetCustomAttribute<SelectorTokenProviderAttribute>();

            return tokenProviders.Append(SelectorTokenProviderAttribute.Default);
        }

        public static IImmutableList<SelectorToken> NearestSelectorTokens(this MemberInfo member)
        {
            var tokenProvider = member.TokenProviders().First();
            var nearestSelectorTokens = tokenProvider.GetSelectorTokens(member).First();
            return nearestSelectorTokens;
        }
    }
}