using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SmartSettingAttribute : Attribute
    {
        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string ProviderName { get; set; }

        // todo for future use
        //public bool Cached { get; set; } = true;
    }
}