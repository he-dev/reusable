using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Collections;
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

    public readonly struct SelectorToken : IEquatable<SelectorToken>
    {
        public SelectorToken(string name, SelectorTokenType tokenType)
        {
            Name = name;
            Type = tokenType;
        }

        public string Name { get; }

        public SelectorTokenType Type { get; }

        public override string ToString() => Name;

        public override int GetHashCode() => AutoEquality<SelectorToken>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is SelectorToken st && Equals(st);

        public bool Equals(SelectorToken obj) => AutoEquality<SelectorToken>.Comparer.Equals(this, obj);

        #region Factories

        public static SelectorToken Scheme(string name) => new SelectorToken(name, SelectorTokenType.Scheme);

        #endregion

        public static implicit operator string(SelectorToken token) => token.ToString();
    }

    public interface ISelectorTokenFactory
    {
        SelectorTokenType TokenType { get; }
    }

    public interface IConstantSelectorTokenFactory : ISelectorTokenFactory
    {
        SelectorToken CreateSelectorToken(MemberInfo member);
    }

    public interface IVariableSelectorTokenFactory : ISelectorTokenFactory
    {
        SelectorToken CreateSelectorToken(MemberInfo member, string parameter);
    }

    [PublicAPI]
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
    public abstract class SelectorTokenFactoryAttribute : Attribute, ISelectorTokenFactory
    {
        public abstract SelectorTokenType TokenType { get; }

        public string? Prefix { get; set; }

        public string? Suffix { get; set; }

        protected string Filter(string name, MemberInfo member)
        {
            return
                member
                    .GetCustomAttributes<SelectorTokenFilterAttribute>(inherit: false) // Get only own filters and not inherited ones.
                    .Aggregate(name, (current, filter) => filter.Apply(current));
        }

        protected SelectorToken CreateSelectorToken(string member) => new SelectorToken($"{Prefix}{member}{Suffix}", TokenType);
    }

    public class UseSchemeAttribute : SelectorTokenFactoryAttribute, IConstantSelectorTokenFactory
    {
        private readonly string _name;

        public UseSchemeAttribute(string name)
        {
            _name = name;
            Suffix = ":";
        }

        public override SelectorTokenType TokenType => Scheme;

        public SelectorToken CreateSelectorToken(MemberInfo member)
        {
            return CreateSelectorToken(_name);
        }
    }

    public class UseNamespaceAttribute : SelectorTokenFactoryAttribute, IConstantSelectorTokenFactory
    {
        private readonly string? _name;

        public UseNamespaceAttribute(string? name = default)
        {
            _name = name;
            Suffix = "+";
        }

        public override SelectorTokenType TokenType => Namespace;

        public SelectorToken CreateSelectorToken(MemberInfo member)
        {
            return CreateSelectorToken(_name ?? member.ReflectedType.Namespace);
        }
    }

    public class UseTypeAttribute : SelectorTokenFactoryAttribute, IConstantSelectorTokenFactory
    {
        private readonly string? _name;

        public UseTypeAttribute(string? name = default)
        {
            _name = name;
            Suffix = ".";
        }

        public override SelectorTokenType TokenType => Type;

        public SelectorToken CreateSelectorToken(MemberInfo member)
        {
            member = member.ReflectedType;
            return CreateSelectorToken(Filter(_name ?? ((Type)member).ToPrettyString(), member));
        }
    }

    public class UseMemberAttribute : SelectorTokenFactoryAttribute, IConstantSelectorTokenFactory
    {
        private readonly string? _name;

        public UseMemberAttribute(string? name = default)
        {
            _name = name;
        }

        public override SelectorTokenType TokenType => Member;

        public SelectorToken CreateSelectorToken(MemberInfo member)
        {
            return CreateSelectorToken(Filter(_name ?? member.Name, member));
        }
    }

    public class UseIndexAttribute : SelectorTokenFactoryAttribute, IVariableSelectorTokenFactory
    {
        public UseIndexAttribute()
        {
            Prefix = "[";
            Suffix = "]";
        }

        public override SelectorTokenType TokenType => Index;

        public SelectorToken CreateSelectorToken(MemberInfo member, string parameter)
        {
            return CreateSelectorToken(parameter);
        }
    }
}