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

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class RenameAttribute : Attribute
    {
        private readonly string _name;

        public RenameAttribute(string name) => _name = name;

        public override string ToString() => _name;
    }

    [PublicAPI]
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class KeyFactoryAttribute : Attribute, IKeyFactory
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public abstract Key CreateKey(Selector selector);

        protected string FixName(string name, MemberInfo member)
        {
            var nameFixes = member.GetCustomAttributes<NameFixAttribute>().ToList();

            return
                nameFixes.Any()
                    ? nameFixes.Aggregate(name, (current, fix) => fix.Apply(current))
                    : name;
        }
    }

    public class UseGlobalAttribute : KeyFactoryAttribute
    {
        private readonly string _prefix;

        public UseGlobalAttribute(string prefix)
        {
            _prefix = prefix;
            Suffix = ":";
        }

        public override Key CreateKey(Selector selector)
        {
            return new Key(_prefix) { Suffix = Suffix };
        }
    }

    public class UseNamespaceAttribute : KeyFactoryAttribute
    {
        public UseNamespaceAttribute()
        {
            Suffix = "+";
        }

        public override Key CreateKey(Selector selector)
        {
            var type = selector.DeclaringType;
            return new Key(type.Namespace) { Suffix = Suffix };
        }
    }

    public class UseTypeAttribute : KeyFactoryAttribute
    {
        public UseTypeAttribute()
        {
            Suffix = ".";
        }

        public override Key CreateKey(Selector selector)
        {
            var type = selector.DeclaringType;

            var typeName =
                type.GetCustomAttribute<RenameAttribute>()?.ToString() is string rename
                    ? rename
                    : FixName(type.ToPrettyString(), type);

            return new Key(typeName) { Suffix = Suffix };
        }
    }

    public class UseMemberAttribute : KeyFactoryAttribute
    {
        public override Key CreateKey(Selector selector)
        {
            var memberName =
                selector.Member.GetCustomAttribute<RenameAttribute>()?.ToString() is string rename
                    ? rename
                    : FixName(selector.Member.Name, selector.Member);

            return new Key(memberName);
        }
    }

    public interface IKeyEnumerator
    {
        // Enumerates keys applied to a property.
        [NotNull, ItemNotNull]
        IEnumerable<IEnumerable<Key>> EnumerateKeys(Selector selector);
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class KeyEnumeratorAttribute : Attribute, IKeyEnumerator
    {
        public static readonly IImmutableList<Type> DefaultOrder =
            ImmutableList<Type>
                .Empty
                .Add(typeof(UseGlobalAttribute))
                .Add(typeof(UseNamespaceAttribute))
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

        public IEnumerable<IEnumerable<Key>> EnumerateKeys(Selector selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return
                from m in selector.Member.AncestorTypesAndSelf()
                where m.IsDefined(typeof(KeyFactoryAttribute))
                let keys =
                (
                    // Key-factories per member must be sorted before they can be used.
                    from f in m.GetCustomAttributes<KeyFactoryAttribute>()
                    orderby _keyTypes[f.GetType()]
                    select f.CreateKey(selector)
                ).ToImmutableList()
                where keys.Any()
                select keys;
        }
    }

    public static class Helpers
    {
        public static IEnumerable<MemberInfo> AncestorTypesAndSelf(this MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            var current = member;
            do
            {
                yield return current;

                if (current is Type type)
                {
                    if (type.IsInterface)
                    {
                        yield break;
                    }

                    if (type.GetProperty(member.Name) is PropertyInfo otherProperty)
                    {
                        yield return otherProperty;
                    }
                }

                current = current.DeclaringType;
            } while (!(current is null));
        }
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class Key
    {
        public Key(string value) => Value = value;

        private string DebuggerDisplay => ToString();

        public string Prefix { get; set; }

        public string Value { get; }

        public string Suffix { get; set; }

        public override string ToString() => $"{Prefix}{Value}{Suffix}";

        [NotNull]
        public static implicit operator string(Key key) => key.Value;

        [NotNull]
        public static implicit operator SoftString(Key key) => (string)key;
    }

    public interface INameFix
    {
        [NotNull]
        string Apply(string name);
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class NameFixAttribute : Attribute, INameFix
    {
        public abstract string Apply(string name);
    }

    public class Unchanged : INameFix
    {
        public string Apply(string name) => name;
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