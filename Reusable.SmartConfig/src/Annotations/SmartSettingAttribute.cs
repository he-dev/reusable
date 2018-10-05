using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.SmartConfig.Annotations
{
    public interface IPrefixable
    {
        string Prefix { get; }
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SettingProviderAttribute : Attribute, IPrefixable
    {
        private string _prefix;
        private SettingNameComplexity _settingNameComplexity;
        private readonly ISet<Type> _providerTypes;
        private readonly ISet<SoftString> _providerNames;

        private SettingProviderAttribute(ISet<Type> providerTypes, ISet<SoftString> providerNames)
        {
            _providerTypes = providerTypes;
            _providerNames = providerNames;
            _settingNameComplexity = SettingNameComplexity.Medium;
        }

        public SettingProviderAttribute(params Type[] providerTypes)
            : this(new HashSet<Type>(providerTypes), default)
        {
        }

        public SettingProviderAttribute(params string[] providerNames)
            : this(default, new HashSet<SoftString>(providerNames.Select(SoftString.Create)))
        {
        }

        [CanBeNull]
        public Type AssemblyType { get; set; }

        [CanBeNull]
        public string Prefix
        {
            get => _prefix ?? (AssemblyType is default ? default : Assembly.GetAssembly(AssemblyType).GetName().Name);
            set => _prefix = value;
        }

        // todo - disallow .Inherit
        public SettingNameComplexity SettingNameComplexity
        {
            get => _settingNameComplexity;
            set => _settingNameComplexity = value;
        }

        public bool Contains<T>(T provider) where T : ISettingProvider => _providerTypes.Contains(typeof(T)) || _providerNames.Contains(provider.Name);
    }

    [UsedImplicitly]
    public class SettingAttribute : Attribute, IPrefixable
    {
        [CanBeNull] private string _prefix;
        private PrefixHandling _prefixHandling = PrefixHandling.Inherit;

        internal SettingAttribute()
        {
        }

        [CanBeNull]
        public string Prefix
        {
            get => _prefix;
            set
            {
                _prefix = value;
                _prefixHandling = _prefix.IsNullOrEmpty() ? PrefixHandling.Inherit : PrefixHandling.Override;
            }
        }

        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string ProviderName { get; set; }

        public SettingNameComplexity Complexity { get; set; } = SettingNameComplexity.Inherit;

        public PrefixHandling PrefixHandling
        {
            get => _prefixHandling;
            set
            {
                _prefixHandling = value;
                if (_prefixHandling == PrefixHandling.Disable)
                {
                    _prefix = default;
                }
            }
        }

        // todo for future use
        //public bool Cache { get; set; } = true;
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingTypeAttribute : SettingAttribute
    {
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingMemberAttribute : SettingAttribute
    {
    }

    public enum PrefixHandling
    {
        Inherit = -1,
        Disable = 0,
        Override = 1,
    }
}