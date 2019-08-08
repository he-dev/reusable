using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Quickey
{
    using static SelectorTokenType;
    
    // [Prefix:][Name.space+][Type.]Member[[Index]]
    // [UsePrefix("blub"), UseNamespace, UseType, UseMember, UseIndex?]

    public enum SelectorTokenType
    {
        Scheme,
        Namespace,
        Type,
        Member,
        Index
    }

    public readonly struct SelectorToken
    {
        public SelectorToken(string name, SelectorTokenType tokenType)
        {
            Name = name;
            Type = tokenType;
        }

        public string Name { get; }

        public SelectorTokenType Type { get; }

        public override string ToString() => Name;

        public static implicit operator string(SelectorToken token) => token.ToString();
    }

    public interface ISelectorTokenFactory
    {
        SelectorTokenType TokenType { get; }
        
        SelectorToken CreateSelectorToken(MemberInfo member, string parameter);
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
    public abstract class SelectorTokenFactoryAttribute : Attribute, ISelectorTokenFactory
    {
        public abstract SelectorTokenType TokenType { get; }
        
        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public abstract SelectorToken CreateSelectorToken(MemberInfo member, string parameter);

        protected string Transform(string name, MemberInfo member)
        {
            return
                member
                    .GetCustomAttributes<SelectorNameTransformAttribute>(false) // Get only own factories and not inherited ones.
                    .Aggregate(name, (current, fix) => fix.Apply(current));
        }

        protected SelectorToken CreateSelectorToken(string member) => new SelectorToken($"{Prefix}{member}{Suffix}", TokenType);
    }

    public class UseSchemeAttribute : SelectorTokenFactoryAttribute
    {
        private readonly string _name;

        public UseSchemeAttribute(string name)
        {
            _name = name;
            Suffix = ":";
        }

        public override SelectorTokenType TokenType => Scheme;

        public override SelectorToken CreateSelectorToken(MemberInfo member, string parameter)
        {
            return CreateSelectorToken(_name);
        }
    }

    public class UseNamespaceAttribute : SelectorTokenFactoryAttribute
    {
        private readonly string _name;

        public UseNamespaceAttribute(string name = default)
        {
            _name = name;
            Suffix = "+";
        }
        
        public override SelectorTokenType TokenType => Namespace;

        public override SelectorToken CreateSelectorToken(MemberInfo member, string parameter)
        {
            return CreateSelectorToken(_name ?? member.DeclaringType.Namespace);
        }
    }

    public class UseTypeAttribute : SelectorTokenFactoryAttribute
    {
        public UseTypeAttribute()
        {
            Suffix = ".";
        }
        
        public override SelectorTokenType TokenType => Type;

        public override SelectorToken CreateSelectorToken(MemberInfo member, string parameter)
        {
            var type = member.DeclaringType;

            var typeName =
                type.GetCustomAttribute<RenameAttribute>()?.ToString() is string rename
                    ? rename
                    : Transform(type.ToPrettyString(), type);

            return CreateSelectorToken(typeName);
        }
    }

    public class UseMemberAttribute : SelectorTokenFactoryAttribute
    {
        public override SelectorTokenType TokenType => Member;
        
        public override SelectorToken CreateSelectorToken(MemberInfo member, string parameter)
        {
            var memberName =
                member.GetCustomAttribute<RenameAttribute>()?.ToString() is string rename
                    ? rename
                    : Transform(member.Name, member);

            return CreateSelectorToken(memberName);
        }
    }

    public class UseIndexAttribute : SelectorTokenFactoryAttribute
    {
        public UseIndexAttribute()
        {
            Prefix = "[";
            Suffix = "]";
        }
        
        public override SelectorTokenType TokenType => Index;

        public override SelectorToken CreateSelectorToken(MemberInfo member, string parameter)
        {
            return CreateSelectorToken(parameter);
        }
    }
}