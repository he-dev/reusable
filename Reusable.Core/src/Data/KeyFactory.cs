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
using Reusable.Reflection;

namespace Reusable.Data
{
    // [Prefix:][Name.space+][Type.]Member[[Index]]
    // [UsePrefix("blub"), UseNamespace, UseType, UseMember, UseIndex?]

    public interface IKeyFactory
    {
        [NotNull]
        Key CreateKey(Selector selector);
    }

    [PublicAPI]
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class KeyFactoryAttribute : Attribute, IKeyFactory
    {
        public abstract Key CreateKey(Selector selector);
    }

    public class UseTypeAttribute : KeyFactoryAttribute
    {
        public string Separator { get; set; } = ".";

        public override Key CreateKey(Selector selector)
        {
            var type = selector.DeclaringType;// ?? throw new InvalidOperationException($"{memberExpression} does not have a {nameof(MemberInfo.DeclaringType)}.");
            var typeName = type.ToPrettyString();
            typeName = type.GetCustomAttributes<NameFixAttribute>().Aggregate(typeName, (name, fix) => fix.Apply(name));
            return new TypeKey(typeName, Separator);
        }
    }

    public class UseMemberAttribute : KeyFactoryAttribute
    {
        public override Key CreateKey(Selector selector)
        {
            return new MemberKey(selector.Property.Name);
        }
    }

    public interface IKeyEnumerator
    {
        // Enumerates keys applied to a property.
        [NotNull, ItemNotNull]
        IEnumerable<Key> EnumerateKeys(Selector selector);
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class KeyEnumeratorAttribute : Attribute, IKeyEnumerator
    {
        public static readonly IImmutableList<Type> DefaultOrder =
            ImmutableList<Type>
                .Empty
                //.Add(typeof(UsePrefixAttribute))
                //.Add(typeof(UseNamespaceAttribute))
                .Add(typeof(UseTypeAttribute))
                .Add(typeof(UseMemberAttribute));


        private readonly IImmutableDictionary<Type, int> _keyTypes;

        public KeyEnumeratorAttribute(params Type[] keyTypes)
        {
            _keyTypes =
                keyTypes
                    .Select((type, index) => (type, index))
                    .ToImmutableDictionary(x => x.type, x => x.index);
        }

        public KeyEnumeratorAttribute() : this(DefaultOrder.ToArray()) { }

        public IEnumerable<Key> EnumerateKeys(Selector selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return
                from m in selector.Property.AncestorTypesAndSelf()
                where m.IsDefined(typeof(KeyFactoryAttribute))
                from f in m.GetCustomAttributes<KeyFactoryAttribute>()
                orderby _keyTypes[f.GetType()]
                select f.CreateKey(selector);
        }
    }
    
    public static class Helpers
    {
        public static IEnumerable<MemberInfo> AncestorTypesAndSelf(this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var current = (MemberInfo)property;
            do
            {
                yield return current;

                if (current is Type type)
                {
                    if (type.IsInterface)
                    {
                        yield break;
                    }

                    if (type.GetProperty(property.Name) is PropertyInfo otherProperty)
                    {
                        yield return otherProperty;
                    }
                }

                current = current.DeclaringType;
            } while (!(current is null));
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

    public interface INameFix
    {
        [NotNull]
        string Apply(string name);
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class NameFixAttribute : Attribute, INameFix
    {
        public abstract string Apply(string name);
    }

    public class RemoveAttribute : NameFixAttribute
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