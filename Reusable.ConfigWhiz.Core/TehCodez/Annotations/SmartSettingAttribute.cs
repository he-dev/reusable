using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Annotations
{
    [UsedImplicitly]
    public class SmartSettingAttribute : Attribute
    {
        // Name = "[member]" => the same as property/field
        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string Datasource { get; set; }
    }
}