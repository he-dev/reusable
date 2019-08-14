using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous.Config.Annotations;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    public static class SettingRequestBuilder
    {
        public static readonly string NameKey = "name";

        public static Request CreateRequest(RequestMethod method, Selector selector, object value = default)
        {
            var resources =
                from m in selector.Member.Path()
                where m.IsDefined(typeof(ResourceAttribute))
                select m.GetCustomAttribute<ResourceAttribute>();

            var resource = resources.FirstOrDefault();

            var uri = UriStringHelper.CreateQuery
            (
                scheme: resource?.Scheme ?? "config",
                path: ImmutableList<string>.Empty.Add("settings"),
                query: ImmutableDictionary<string, string>.Empty.Add("name", selector.ToString())
            );

            return new Request
            {
                Uri = uri,
                Method = method,
                Metadata =
                    ImmutableContainer
                        .Empty
                        .SetItem(ResourceProperties.DataType, selector.DataType)
                        // request.Properties.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Type)) == typeof(string)
                        //.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), resource?.Provider.ToSoftString())
                        //.SetItem(ResourceProperty.ActualName, $"[{selector.Join(x => x.ToString(), ", ")}]")
                        .SetItemWhen(ResourceControllerProperties.Tags, ImmutableHashSet<SoftString>.Empty.Add(resource?.Provider.ToSoftString()), _ => resource != null),
                        //.AddTag(resource?.Provider.ToSoftString()),
                Body = value
            };
        }
    }
}