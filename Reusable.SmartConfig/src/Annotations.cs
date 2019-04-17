using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourcePrefixAttribute : Attribute
    {
        public ResourcePrefixAttribute(string name) => Name = name;

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceSchemeAttribute : Attribute
    {
        public ResourceSchemeAttribute(string name) => Name = name;

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceNameAttribute : Attribute
    {
        public ResourceNameAttribute() { }

        public ResourceNameAttribute(string name) => Name = name;

        [CanBeNull]
        public string Name { get; }

        public ResourceNameLevel Level { get; set; } = ResourceNameLevel.TypeMember;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceProviderAttribute : Attribute
    {
        public ResourceProviderAttribute([NotNull] string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        public ResourceProviderAttribute([NotNull] Type type) => Type = type ?? throw new ArgumentNullException(nameof(type));

        public string Name { get; }

        public Type Type { get; }
    }

    public static class SettingQueryStringKeys
    {
        public const string Prefix = nameof(Prefix);
        public const string Handle = nameof(Handle);
        public const string Level = nameof(Level);
        public const string IsCollection = nameof(IsCollection);
    }

    public enum ResourceNameLevel
    {
        NamespaceTypeMember,
        TypeMember,
        Member,
    }
}