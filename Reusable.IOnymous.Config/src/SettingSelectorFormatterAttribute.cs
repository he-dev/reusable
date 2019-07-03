using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.IOnymous.Config.Annotations;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    /// <summary>
    /// Formats setting selector as uri-string: config:///settings?name=Global:Name.space+Type.Member[Index] 
    /// </summary>
    public class SettingSelectorFormatterAttribute : SelectorFormatterAttribute
    {
        public override string Format(Selector selector)
        {
            var plainKey = selector.Join(string.Empty);
            var resources =
                from m in selector.Members()
                where m.IsDefined(typeof(ResourceAttribute))
                select m.GetCustomAttribute<ResourceAttribute>();
            var resource = resources.FirstOrDefault();
            return UriStringHelper.CreateQuery
            (
                scheme: resource?.Scheme?.ToString() ?? "config",
                path: ImmutableList<string>.Empty.Add("settings"),
                query: ImmutableDictionary<string, string>.Empty.Add("name", plainKey)
            );
        }
    }
}