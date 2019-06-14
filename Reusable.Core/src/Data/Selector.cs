using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using linq = System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Data
{
    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }

    [PublicAPI]
    public static class From<T> where T : INamespace
    {
        public static Selector<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }

    [PublicAPI]
    public class Selector
    {
        private readonly bool _containsIndex;

        public Selector(LambdaExpression selector)
        {
            Expression = selector;
            (DeclaringType, Instance, Member) = MemberVisitor.GetMemberInfo(selector);
            Keys = this.GetKeys().ToImmutableList();
            if (Keys.Empty()) throw new ArgumentException($"'{selector}' does not specify which keys to use.");
        }

        protected Selector(LambdaExpression selector, IImmutableList<Key> keys) : this(selector)
        {
            Keys = keys;
            _containsIndex = true;
        }

        public LambdaExpression Expression { get; }

        public Type DeclaringType { get; } // => Expression.ToMemberExpression().Member.DeclaringType;

        public object Instance { get; }

        public MemberInfo Member { get; } // => (PropertyInfo)Expression.ToMemberExpression().Member;

        public Type MemberType
        {
            get
            {
                switch (Member)
                {
                    case PropertyInfo property: return property.PropertyType;
                    case FieldInfo field: return field.FieldType;
                    default: throw new ArgumentOutOfRangeException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
                }
            }
        }

        [NotNull, ItemNotNull]
        public IImmutableList<Key> Keys { get; }

        public static Selector FromProperty(Type declaringType, PropertyInfo property)
        {
            // () => x.Member
            var selector =
                linq.Expression.Lambda(
                    linq.Expression.Property(
                        linq.Expression.Constant(default, declaringType),
                        property.Name
                    )
                );

            return new Selector(selector);
        }

        public override string ToString()
        {
            var formatters =
                from m in Member.AncestorTypesAndSelf()
                where m.IsDefined(typeof(SelectorFormatterAttribute))
                select m.GetCustomAttribute<SelectorFormatterAttribute>();

            return
                formatters
                    .FirstOrDefault()?
                    .Format(this)
                ?? throw DynamicException.Create("SelectorFormatterNotFound", $"'{Expression.ToPrettyString()}' must specify a {nameof(SelectorFormatterAttribute)}");
        }

        public virtual Selector Index(string index, string prefix = "[", string suffix = "]")
        {
            if (_containsIndex) throw new InvalidOperationException($"'{Expression}' already contains an index.");
            return new Selector(Expression, Keys.Add(new Key(index) { Prefix = prefix, Suffix = suffix }));
        }

        [NotNull]
        public static implicit operator string(Selector selector) => selector.ToString();

        [NotNull]
        public static implicit operator SoftString(Selector selector) => selector.ToString();
    }

    public class Selector<T> : Selector
    {
        public Selector(LambdaExpression selector) : base(selector) { }

        //private Selector(Selector<T> other, IImmutableList<Key> keys) : base(other, keys) { }

        //        public override Selector<T> Index(string index)
        //        {
        //            if (Keys.OfType<IndexKey>().Any()) throw new InvalidOperationException("This selector already contains an index.");
        //
        //            return new Selector<T>(this, Keys.Add(new IndexKey(index)));
        //        }

        public static implicit operator Selector<T>(Expression<Func<T>> selector) => new Selector<T>(selector);
    }

    [PublicAPI]
    public interface ISelectorFormatter
    {
        string Format(Selector selector);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public abstract class SelectorFormatterAttribute : Attribute, ISelectorFormatter
    {
        public abstract string Format(Selector selector);
    }

    public class PlainSelectorFormatterAttribute : SelectorFormatterAttribute
    {
        public override string Format(Selector selector)
        {
            return selector.Keys.Join(string.Empty);
        }
    }

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

        public static IEnumerable<Selector> Where<T>(this IEnumerable<Selector> featureSelectors, params string[] names) where T : Attribute, IEnumerable<string>
        {
            if (!names.Any()) throw new ArgumentException("You need to specify at least one tag.");

            return
                from f in featureSelectors
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