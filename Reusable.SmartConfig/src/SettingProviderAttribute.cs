using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class SettingProviderAttribute : Attribute
    {
        private readonly IImmutableSet<SoftString> _providerNames;

        private SettingProviderAttribute
        (
            SettingNameStrength strength,
            IEnumerable<Type> providerTypes,
            IEnumerable<string> providerNames
        )
        {
            _providerNames = providerNames.Concat(providerTypes.Select<Type, string>(t => t.ToPrettyString())).Select(SoftString.Create).ToImmutableHashSet();
            Strength = strength;
        }

        public SettingProviderAttribute(SettingNameStrength strength, params Type[] providerTypes)
            : this(strength, providerTypes.Select(t => t.ToPrettyString()).ToArray())
        {
        }

        public SettingProviderAttribute(SettingNameStrength strength, params string[] providerNames)
            : this(strength, Enumerable.Empty<Type>(), providerNames)
        {
            if (strength == SettingNameStrength.Inherit) throw new ArgumentOutOfRangeException(paramName: nameof(strength), $"{nameof(strength)} must not be '{SettingNameStrength.Inherit}'.");
            if (providerNames?.Any() == false) throw new ArgumentException(paramName: nameof(providerNames), message: "You need to specify at least one provider type or name.");
        }

        public static readonly SettingProviderAttribute Default = new SettingProviderAttribute
        (
            SettingNameStrength.Medium,
            Enumerable.Empty<Type>(),
            Enumerable.Empty<string>()
        );

        [CanBeNull]
        public string Prefix { get; set; }

        public PrefixHandling PrefixHandling => string.IsNullOrWhiteSpace(Prefix) ? PrefixHandling.Disable : PrefixHandling.Enable;

        public SettingNameStrength Strength { get; }

        public int ProviderNameCount => _providerNames.Count;

        public bool Matches<T>(T provider) where T : IResourceProvider
        {
            return
                _providerNames
                    .Intersect(provider.Metadata.ProviderNames().Where(name => !name.IsNullOrEmpty()))
                    .Any();
        }
    }
}