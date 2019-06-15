using System;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.Data;
using Reusable.Quickey;
using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig
{
    /// <summary>
    /// Formats setting selector as uri-string: config:///settings?name=Global:Name.space+Type.Member[Index] 
    /// </summary>
    public class SettingSelectorFormatterAttribute : SelectorFormatterAttribute
    {
        public override string Format(Selector selector)
        {
            var plainKey = selector.Keys.Join(string.Empty);
            var resources =
                from m in selector.Member.AncestorTypesAndSelf()
                where m.IsDefined(typeof(ResourceAttribute))
                select m.GetCustomAttribute<ResourceAttribute>();
            var resource = resources.FirstOrDefault();
            return $"{resource?.Scheme?.ToString() ?? "config"}:///settings?name={Uri.EscapeDataString(plainKey)}";
        }
    }
}