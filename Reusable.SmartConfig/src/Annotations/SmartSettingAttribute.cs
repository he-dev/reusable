using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            : this(new HashSet<Type>(providerTypes), new HashSet<SoftString>())
        {
        }

        public SettingProviderAttribute(params string[] providerNames)
            : this(new HashSet<Type>(), new HashSet<SoftString>(providerNames.Select(SoftString.Create)))
        {
        }
        
        public static readonly SettingProviderAttribute Default = new SettingProviderAttribute(new HashSet<Type>(), new HashSet<SoftString>())
        {
            SettingNameComplexity = SettingNameComplexity.Medium,
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
        
//        public static SettingAttribute Default { get; } = new SettingAttribute
//        {
//            Complexity = SettingNameComplexity.Medium,
//            PrefixHandling = PrefixHandling.Disable
//        };

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