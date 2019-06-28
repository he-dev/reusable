using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Reflection;
using linq = System.Linq.Expressions;

namespace Reusable.Quickey
{
    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }

    [PublicAPI]
    public class From<T> //where T : INamespace
    {
        public static From<T> This => default;
        
        [NotNull]
        public static Selector<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }

    public static class FromExtensions
    {
        [NotNull]
        public static Selector<TMember> Select<T, TMember>(this From<T> from, [NotNull] Expression<Func<T, TMember>> selector) //where T : INamespace
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

        public Selector Index(string index, string prefix = "[", string suffix = "]")
        {
            if (_containsIndex) throw new InvalidOperationException($"'{Expression}' already contains an index.");
            return new Selector(Expression, Keys.Add(new Key(index) { Prefix = prefix, Suffix = suffix }));
        }

        //[NotNull]
        //public static implicit operator string(Selector selector) => selector.ToString();

        //[NotNull]
        //public static implicit operator SoftString(Selector selector) => selector.ToString();
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
}