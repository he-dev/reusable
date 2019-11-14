using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Reflection;
using linq = System.Linq.Expressions;

namespace Reusable.Quickey
{
    [PublicAPI]
    public class From<T>
    {
        /// <summary>
        /// Gets the current From that can be used later for selection.
        /// </summary>
        public static From<T> This => default;

        [NotNull]
        public static Selector<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
        {
            return This.Select(selector);
        }
    }

    public static class FromExtensions
    {
        [NotNull]
        public static Selector<TMember> Select<T, TMember>(this From<T> from, [NotNull] Expression<Func<T, TMember>> selector)
        {
            return new Selector<TMember>(selector);
        }

        [NotNull]
        public static Selector<TMember> Select<T, TMember>(this From<T> from, [NotNull] Expression<Func<Selector<TMember>>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }

    [PublicAPI]
    public class Selector : IEnumerable<SelectorToken>
    {
        private readonly IEnumerable<SelectorToken> _selectorTokens;
        private readonly ISelectorFormatter _formatter;

        public Selector(LambdaExpression selector)
        {
            Expression = selector;
            (ReflectedType, Instance, Member) = MemberVisitor.GetMemberInfo(selector);

            // The first one is the closest one to the member.
            _selectorTokens = Member.NearestSelectorTokens();
            if (_selectorTokens is null || _selectorTokens.Empty())
            {
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' does not specify any selectors.");
            }

            if (_selectorTokens.GroupBy(t => t.Type).Any(g => g.Count() > 1))
            {
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' may specify each selector only once.");
            }

            if (_selectorTokens.All(t => t.Type != SelectorTokenType.Member))
            {
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' must specify at least the '{nameof(UseMemberAttribute)}'.");
            }

            var formatters =
                from m in Member.Path()
                where m.IsDefined(typeof(SelectorFormatterAttribute))
                select m.GetCustomAttribute<SelectorFormatterAttribute>();

            // Use the plain-formatter as fallback.
            _formatter = formatters.Append(new PlainSelectorFormatterAttribute()).First();
        }

        public Selector(LambdaExpression selector, IEnumerable<SelectorToken> tokens) : this(selector)
        {
            _selectorTokens = tokens.ToImmutableList();
        }

        public LambdaExpression Expression { get; }

        public Type ReflectedType { get; }

        public object Instance { get; }

        public MemberInfo Member { get; }

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

        public Type DataType => typeof(Selector).IsAssignableFrom(MemberType) ? MemberType.GetGenericArguments().Single() : MemberType;

        public static Selector FromMember(Type declaringType, MemberInfo member)
        {
            // todo - this might require improved static/instance handling

            var memberAccess = default(Expression);

            if (member is PropertyInfo property)
            {
                memberAccess =
                    property.GetGetMethod().IsStatic
                        ? linq.Expression.Property(default, property)
                        : linq.Expression.Property(linq.Expression.Constant(default, declaringType), property);
            }

            if (member is FieldInfo field)
            {
                memberAccess =
                    field.IsStatic
                        ? linq.Expression.Field(default, field)
                        : linq.Expression.Field(linq.Expression.Constant(default, declaringType), field);
            }

            // () => x.Member
            var selector = linq.Expression.Lambda(memberAccess);

            return new Selector(selector);
        }

        public override string ToString() => _formatter.Format(this);

        public IEnumerator<SelectorToken> GetEnumerator() => _selectorTokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_selectorTokens).GetEnumerator();

        //[NotNull]
        //public static implicit operator string(Selector selector) => selector.ToString();

        //[NotNull]
        //public static implicit operator SoftString(Selector selector) => selector.ToString();
    }

    public class Selector<T> : Selector
    {
        public Selector(LambdaExpression selector) : base(selector) { }

        public static implicit operator Selector<T>(Expression<Func<T>> selector) => new Selector<T>(selector);
    }

    public class StringSelector<T> : Selector<T>
    {
        public StringSelector(Selector selector) : base(selector.Expression) { }

        public static implicit operator string(StringSelector<T> stringSelector) => stringSelector.ToString();

        //public static implicit operator StringSelector<T>(Selector<T> selector) => selector.AsString();
    }

    [PublicAPI]
    public abstract class SelectorBuilder<T>
    {
        public static IEnumerable<Selector> Selectors
        {
            get
            {
                var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance).Concat(typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Static));
                return
                    from m in members
                    where m is PropertyInfo || m is FieldInfo
                    select Selector.FromMember(typeof(T), m);
            }
        }

        [NotNull]
        public static Selector<TMember> Select<TMember>([NotNull] Expression<Func<Selector<TMember>>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }
}