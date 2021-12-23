using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Essentials.Reflection;

namespace Reusable.Quickey
{
    using expr = System.Linq.Expressions.Expression;


    [PublicAPI]
    public class Selector : IEnumerable<SelectorToken>
    {
        private readonly IEnumerable<SelectorToken> _tokens;
        private readonly ISelectorFormatter _formatter;

        public Selector(LambdaExpression expression, params ISelectorTokenFactoryParameter[] parameters)
        {
            Metadata = MemberVisitor.GetMemberInfo(expression);

            _tokens =
                from f in FirstSelectorTokenProviderOrDefault(Metadata.Member).GetSelectorTokenFactories(Metadata.Member)
                select f.CreateSelectorToken(new SelectorContext
                {
                    Metadata = Metadata,
                    TokenParameters = { parameters }
                });

            _tokens = _tokens.ToList();

            if (_tokens.Empty())
            {
                throw new ArgumentException(paramName: nameof(expression), message: $"'{expression}' does not specify any tokens.");
            }

            _formatter = FirstSelectorFormatterOrDefault(Metadata.Member);
        }

        public Selector(Type declaringType, MemberInfo member, params ISelectorTokenFactoryParameter[] parameters)
            : this(CreateLambdaExpression(declaringType, member), parameters) { }

        public MemberMetadata Metadata { get; }

        public Type MemberType =>
            typeof(Selector).IsAssignableFrom(Metadata.MemberType)
                ? Metadata.MemberType.GetGenericArguments().Single()
                : Metadata.MemberType;

        private static LambdaExpression CreateLambdaExpression(Type declaringType, MemberInfo member)
        {
            // () => instance.Member
            // () => Type.Member
            var memberAccess = member switch
            {
                PropertyInfo property => expr.Property(property.GetGetMethod().IsStatic ? default : expr.Constant(default, declaringType), property),
                FieldInfo field => expr.Field(field.IsStatic ? default : expr.Constant(default, declaringType), field),
                _ => throw new ArgumentOutOfRangeException()
            };

            return expr.Lambda(memberAccess);
        }

        public static Selector<T> For<T>(Expression<Func<T>> expression, params ISelectorTokenFactoryParameter[] parameters)
        {
            return new Selector<T>(expression, parameters);
        }

        public override string ToString() => _formatter.Format(this);

        public IEnumerator<SelectorToken> GetEnumerator() => _tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_tokens).GetEnumerator();

        public static ISelectorTokenFactoryProvider FirstSelectorTokenProviderOrDefault(MemberInfo member)
        {
            foreach (var m in SelectorPath.Enumerate(member))
            {
                // The first one is the closest one to the member.
                if (m.GetCustomAttribute<SelectorTokenFactoryProviderAttribute>() is {} p)
                {
                    return p;
                }
            }

            return SelectorTokenFactoryProviderAttribute.Default;
        }

        public static ISelectorFormatter FirstSelectorFormatterOrDefault(MemberInfo member)
        {
            foreach (var m in SelectorPath.Enumerate(member))
            {
                if (m.GetCustomAttribute<SelectorFormatterAttribute>() is {} f)
                {
                    return f;
                }
            }

            // Use the this formatter as fallback.
            return new JoinSelectorTokensAttribute();
        }
    }

    public class Selector<T> : Selector
    {
        public Selector(LambdaExpression expression, params ISelectorTokenFactoryParameter[] parameters) : base(expression, parameters) { }
    }

    [PublicAPI]
    public abstract class From<T>
    {
        private From() { }

        /// <summary>
        /// Gets the current From that can be used later for selection.
        /// </summary>
        public static From<T>? This => default;

        public static Selector<TMember> Select<TMember>(Expression<Func<T, TMember>> expression, params ISelectorTokenFactoryParameter[] parameters)
        {
            return new Selector<TMember>(expression, parameters);
        }

        public static Selector<TMember> Select<TMember>(Expression<Func<Selector<TMember>>> expression, params ISelectorTokenFactoryParameter[] parameters)
        {
            return new Selector<TMember>(expression, parameters);
        }
    }

    public static class FromExtensions
    {
        public static Selector<TMember> Select<T, TMember>(this From<T>? from, Expression<Func<T, TMember>> expression, params ISelectorTokenFactoryParameter[] parameters)
        {
            return new Selector<TMember>(expression, parameters);
        }

        public static Selector<TMember> Select<T, TMember>(this From<T>? from, Expression<Func<Selector<TMember>>> expression, params ISelectorTokenFactoryParameter[] parameters)
        {
            return new Selector<TMember>(expression, parameters);
        }
    }

    public class SelectorAttribute : Attribute
    {
        public bool Ignore { get; set; }
    }
}