using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Annotations
{
    [UsedImplicitly]
    public class SmartSettingAttribute : Attribute
    {
        // Name = "[member]" => the same as property/field
        [CanBeNull]
        public CaseInsensitiveString Name { get; set; }

        [CanBeNull]
        public CaseInsensitiveString Datasource { get; set; }
    }
}