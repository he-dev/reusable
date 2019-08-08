using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
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
    }

    [PublicAPI]
    public class Selector : IEnumerable<SelectorToken>
    {
        private readonly IEnumerable<SelectorToken> _selectorTokens;
        private readonly ISelectorFormatter _formatter;

        public Selector(LambdaExpression selector)
        {
            Expression = selector;
            (DeclaringType, Instance, Member) = MemberVisitor.GetMemberInfo(selector);

            // The first one is the closest one to the member.
            _selectorTokens = Member.NearestSelectorTokens(includeIndex: false);
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
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' must specify '{nameof(UseMemberAttribute)}'.");
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

        public Type DeclaringType { get; }

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
                memberAccess = linq.Expression.Property(linq.Expression.Constant(default, declaringType), property);
            }

            if (member is FieldInfo fieldInfo)
            {
                memberAccess = linq.Expression.Field(default, fieldInfo);
            }

            // () => x.Member
            var selector = linq.Expression.Lambda(memberAccess);

            return new Selector(selector);
        }

        // [CanBeNull]
        // private static IImmutableList<SelectorToken> GetSelectorTokens(Selector selector)
        // {
        //     var selectorNameEnumerators =
        //         from m in selector.Path()
        //         where m.IsDefined(typeof(SelectorTokenProviderAttribute))
        //         select m.GetCustomAttribute<SelectorTokenProviderAttribute>();
        //
        //     // The first enumerator is the closest one to the member.
        //     // The first selector-group is the closest one to the member.
        //     return (selectorNameEnumerators.FirstOrDefault() ?? SelectorTokenProviderAttribute.Default).GetSelectorTokens(selector).FirstOrDefault();
        // }

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
                var members =
                    typeof(SelectorBuilder<T>).IsAssignableFrom(typeof(T))
                        ? typeof(T).GetFields().Cast<MemberInfo>()
                        : typeof(T).GetProperties().Cast<MemberInfo>();
                return
                    from p in members
                    select Selector.FromMember(typeof(T), p);
            }
        }

        [NotNull]
        protected static Selector<TMember> Select<TMember>([NotNull] Expression<Func<Selector<TMember>>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }
}