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
    public class Selector : IEnumerable<SelectorName>
    {
        private readonly IEnumerable<SelectorName> _selectorNames;

        public Selector(LambdaExpression selector)
        {
            Expression = selector;
            (DeclaringType, Instance, Member) = MemberVisitor.GetMemberInfo(selector);

            // The first one is the closest one to the member.
            _selectorNames = GetSelectorNames(this);
            if (_selectorNames is null || _selectorNames.Empty())
            {
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' does not specify any selectors.");
            }

            if (_selectorNames.GroupBy(sn => sn.Type).Any(g => g.Count() > 1))
            {
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' may specify each selector only once.");
            }

            if (_selectorNames.All(sn => sn.Type != typeof(UseMemberAttribute)))
            {
                throw new ArgumentException(paramName: nameof(selector), message: $"'{selector.ToPrettyString()}' must specify '{nameof(UseMemberAttribute)}'.");
            }
        }

        public Selector(LambdaExpression selector, IEnumerable<SelectorName> names) : this(selector)
        {
            _selectorNames = names.ToImmutableList();
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

        [CanBeNull, ItemNotNull]
        private static IImmutableList<SelectorName> GetSelectorNames(Selector selector)
        {
            var selectorNameEnumerators =
                from m in selector.Members()
                where m.IsDefined(typeof(SelectorNameEnumeratorAttribute))
                select m.GetCustomAttribute<SelectorNameEnumeratorAttribute>();

            // The first enumerator is the closest one to the member.
            // The first selector-group is the closest one to the member.
            return (selectorNameEnumerators.FirstOrDefault() ?? SelectorNameEnumeratorAttribute.Default).EnumerateSelectorNames(selector).FirstOrDefault();
        }

        public override string ToString()
        {
            var formatters =
                from m in this.Members()
                where m.IsDefined(typeof(SelectorFormatterAttribute))
                select m.GetCustomAttribute<SelectorFormatterAttribute>();

            // Use the plain-formatter as fallback.
            return (formatters.FirstOrDefault() ?? new PlainSelectorFormatterAttribute()).Format(this);
        }

        public IEnumerator<SelectorName> GetEnumerator() => _selectorNames.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_selectorNames).GetEnumerator();

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