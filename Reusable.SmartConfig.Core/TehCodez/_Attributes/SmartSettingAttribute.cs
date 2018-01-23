using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    [UsedImplicitly]
    public class SmartSettingAttribute : Attribute
    {
        // Name = "[member]" => the same as property/field
        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string DatastoreName { get; set; }
    }
}