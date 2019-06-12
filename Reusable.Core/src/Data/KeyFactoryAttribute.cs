using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    // [Prefix:][Name.space+][Type.]Member[[Index]]
    // [UsePrefix("blub"), UseNamespace, UseType, UseMember, UseIndex?]

    public interface IKeyFactory
    {
        [NotNull]
        Key CreateKey(LambdaExpression selector);
    }

    [PublicAPI]
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class KeyFactoryAttribute : Attribute, IKeyFactory
    {
        public abstract Key CreateKey(LambdaExpression selector);
    }

    public class UseTypeAttribute : KeyFactoryAttribute
    {
        public string Separator { get; set; } = ".";

        public override Key CreateKey(LambdaExpression selector)
        {
            var memberExpression = selector.ToMemberExpression();
            var type = memberExpression.Member.DeclaringType ?? throw new InvalidOperationException($"{memberExpression} does not have a {nameof(MemberInfo.DeclaringType)}.");
            var typeName = type.ToPrettyString();
            typeName = type.GetCustomAttributes<TypeNameFixAttribute>().Aggregate(typeName, (name, fix) => fix.Apply(name));
            return new TypeKey(typeName, Separator);
        }
    }

    public class UseMemberAttribute : KeyFactoryAttribute
    {
        public override Key CreateKey(LambdaExpression selector)
        {
            return new MemberKey(selector.ToMemberExpression().Member.Name);
        }
    }

    public interface IKeyEnumerator
    {
        [NotNull, ItemNotNull]
        IEnumerable<Key> EnumerateKeys(LambdaExpression selector);
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class KeyEnumeratorAttribute : Attribute, IKeyEnumerator
    {
        public static readonly IImmutableList<Type> DefaultOrder =
            ImmutableList<Type>
                .Empty
                .Add(typeof(UseTypeAttribute))
                .Add(typeof(UseMemberAttribute));
        
        private readonly IEnumerable<Type> _keyTypes;

        public KeyEnumeratorAttribute(params Type[] keyTypes)
        {
            _keyTypes = keyTypes;
        }

        public KeyEnumeratorAttribute() : this(DefaultOrder.ToArray()) { }

        public IEnumerable<Key> EnumerateKeys(LambdaExpression selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var member = selector.ToMemberExpression().Member;

            foreach (var keyType in _keyTypes)
            {
                // Member's attribute has a higher priority and can override type's default factory.
                if (member.GetCustomAttribute(keyType) is IKeyFactory memberKeyFactory)
                {
                    yield return memberKeyFactory.CreateKey(selector);
                }
                else
                {
                    if (member.DeclaringType?.GetCustomAttribute(keyType) is IKeyFactory typeKeyFactory)
                    {
                        yield return typeKeyFactory.CreateKey(selector);
                    }
                }
            }
        }
    }

    public static class LambdaExpressionExtensions
    {
        [NotNull, ItemNotNull]
        public static IEnumerable<Key> GetKeys(this LambdaExpression selector)
        {
            var member = selector.ToMemberExpression().Member;
            var keyEnumerator =
                member
                    .DeclaringType?
                    .GetCustomAttribute<KeyEnumeratorAttribute>()
                ?? new KeyEnumeratorAttribute(); // throw new InvalidOperationException($"Could not get {nameof(KeyEnumeratorAttribute)} for {selector}.");
            return keyEnumerator.EnumerateKeys(selector);
        }
    }

    [PublicAPI]
    public abstract class Key
    {
        protected Key(string value) => Value = value;

        public string Value { get; }

        public override string ToString() => Value;

        [NotNull]
        public static implicit operator string(Key key) => key.Value;

        [NotNull]
        public static implicit operator SoftString(Key key) => (string)key;
    }

    public class PrefixKey : Key
    {
        public PrefixKey(string value) : base(value) { }
    }

    public class NamespaceKey : Key
    {
        public NamespaceKey(string value) : base(value) { }
    }

    public class TypeKey : Key
    {
        public TypeKey(string value, string separator) : base(value)
        {
            Separator = separator;
        }

        public string Separator { get; }

        public override string ToString() => Value + Separator;
    }

    public class MemberKey : Key
    {
        public MemberKey(string value) : base(value) { }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class IndexKey : Key
    {
        public IndexKey(string value) : base(value) { }

        //private string DebuggerDisplay => $"{_key} Index = {this}";

        public override string ToString() => $"[{Value}]";
    }

    public static class KeyExtensions
    {
        // public static Selector<T> Index<T>(this Selector<T> selector, string index)
        // {
        //     return new Selector<T>(selector.Keys.Add(new IndexKey(index)));
        // }
    }

    public interface ITypeNameFix
    {
        [NotNull]
        string Apply(string name);
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class TypeNameFixAttribute : Attribute, ITypeNameFix
    {
        public abstract string Apply(string name);
    }

    public class RemoveAttribute : TypeNameFixAttribute
    {
        private readonly IEnumerable<string> _patterns;

        public RemoveAttribute([RegexPattern] params string[] patterns) => _patterns = patterns;

        public override string Apply(string name)
        {
            return Regex.Replace(name, $"({_patterns.Join("|")})", string.Empty);
        }
    }

    public class TrimEndAttribute : RemoveAttribute
    {
        public TrimEndAttribute([RegexPattern] string suffixPattern) : base($"{suffixPattern}$") { }
    }

    public class TrimStartAttribute : RemoveAttribute
    {
        public TrimStartAttribute([RegexPattern] string prefixPattern) : base($"^{prefixPattern}") { }
    }
}