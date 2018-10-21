using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.SmartConfig.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class SettingProviderAttribute : Attribute
    {
        private string _prefix;
        private SettingNameStrength _settingNameStrength;
        private readonly ISet<Type> _providerTypes;
        private readonly ISet<SoftString> _providerNames;

        private SettingProviderAttribute(ISet<Type> providerTypes, ISet<SoftString> providerNames)
        {
            _providerTypes = providerTypes;
            _providerNames = providerNames;
            _settingNameStrength = SettingNameStrength.Medium;
        }

        public SettingProviderAttribute(params Type[] providerTypes)
            : this(new HashSet<Type>(providerTypes), new HashSet<SoftString>())
        {
        }

        public SettingProviderAttribute(params string[] providerNames)
            : this(new HashSet<Type>(), new HashSet<SoftString>(providerNames.Select(SoftString.Create)))
        {
        }

        public static readonly SettingProviderAttribute Default = new SettingProviderAttribute(new HashSet<Type>(), new HashSet<SoftString>())
        {
            SettingNameStrength = SettingNameStrength.Medium,
        };

        [CanBeNull]
        public Type AssemblyType { get; set; }

        [CanBeNull]
        public string Prefix
        {
            get => _prefix ?? (AssemblyType is null ? default : Assembly.GetAssembly(AssemblyType).GetName().Name);
            set => _prefix = value;
        }

        // todo - disallow .Inherit
        public SettingNameStrength SettingNameStrength
        {
            get => _settingNameStrength;
            set => _settingNameStrength = value;
        }

        public bool Contains<T>(T provider) where T : ISettingProvider => _providerTypes.Contains(provider.GetType()) || _providerNames.Contains(provider.Name);
    }

    [UsedImplicitly]
    public class SettingAttribute : Attribute
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
                _prefixHandling = _prefix.IsNullOrEmpty() ? PrefixHandling.Inherit : PrefixHandling.Enable;
            }
        }

        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string ProviderName { get; set; }

        [CanBeNull]
        public Type ProviderType { get; set; }

        public SettingNameStrength Strength { get; set; } = SettingNameStrength.Inherit;

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
        Enable = 1,
    }

    public enum ProviderSearch
    {
        /// <summary>
        /// Uses the specified provider, otherwise picks the first setting.
        /// </summary>
        Auto,

        /// <summary>
        /// Ignores any provider name and picks the first setting.
        /// </summary>
        FirstMatch,
    }
}