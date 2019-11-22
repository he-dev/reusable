using System;
using JetBrains.Annotations;

namespace Reusable.Translucent.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class SettingAttribute : Attribute
    {
        public string Controller { get; set; }
    }
}