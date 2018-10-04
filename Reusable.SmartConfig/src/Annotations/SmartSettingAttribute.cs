using System;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.SmartConfig.Annotations
{
    [UsedImplicitly]
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingAttribute : Attribute
    {
        internal SettingAttribute()
        {
        }

        [CanBeNull]
        public string Prefix { get; set; }

        [CanBeNull]
        public bool? PrefixEnabled { get; set; }
        
        [CanBeNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public string ProviderName { get; set; }

        [CanBeNull]
        public SettingNameComplexity? Complexity { get; set; }

        // todo for future use
        //public bool Cached { get; set; } = true;
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SettingAssemblyAttribute : SettingAttribute
    {
        public override string Name
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
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
}