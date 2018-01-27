using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SmartSettingAttribute : Attribute
    {
        // Name = "[member]" => the same as property/field
        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string DataStoreName { get; set; }

        public bool Cached { get; set; } = true;
    }
}