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
    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }

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

        public static IEnumerable<Selector> Selectors<T>(this From<T> from)
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

    [PublicAPI]
    public class Selector : IEnumerable<SelectorName>
    {
        private readonly IEnumerable<SelectorName> _selectorNames;

        public Selector(LambdaExpression selector)
        {
            Expression = selector;
            (DeclaringType, Instance, Member) = MemberVisitor.GetMemberInfo(selector);
            _selectorNames = this.GetSelectorNames();
            if (_selectorNames.Empty()) throw new ArgumentException($"'{selector}' does not specify which keys to use.");
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

        public override string ToString()
        {
            var formatters =
                from m in this.Members()
                where m.IsDefined(typeof(SelectorFormatterAttribute))
                select m.GetCustomAttribute<SelectorFormatterAttribute>();

            return
                formatters
                    .FirstOrDefault()?
                    .Format(this)
                ?? throw DynamicException.Create("SelectorFormatterNotFound", $"'{Expression.ToPrettyString()}' must specify a {nameof(SelectorFormatterAttribute)}");
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

    [PublicAPI]
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public abstract class SelectorBuilder<T>
    {
        public static readonly From<T> This = From<T>.This;

        [NotNull]
        protected static Selector<TMember> Select<TMember>([NotNull] Expression<Func<Selector<TMember>>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }
}