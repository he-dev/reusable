using System;
using System.Linq;
using System.Linq.Custom;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.Quickey;

namespace Reusable.Commander.Services
{
    internal static class ItemRequestFactory
    {
        // arg:///file/f?&position=1
        public static (UriString Uri, IImmutableSession Metadata) CreateItemRequest(CommandParameterMetadata item)
        {
            var queryParameters = new (SoftString Key, SoftString Value)[]
            {
                (CommandArgumentQueryStringKeys.Position, item.Position.ToString()),
                //(CommandArgumentQueryStringKeys.IsCollection, item.IsCollection.ToString()),
            };
            var path = item.Id.Join("/").ToLower();
            var query =
                queryParameters
                    .Where(x => x.Value)
                    .Select(x => $"{x.Key.ToString()}={x.Value.ToString()}")
                    .Join("&");
            query = (SoftString)query ? $"?{query}" : string.Empty;
            return
            (
                $"arg:///{path}{query}",
                ImmutableSession
                    .Empty
                    .SetItem(From<IProviderMeta>.Select(x => x.ProviderName), nameof(CommandArgumentProvider))
                    .SetItem(From<ICommandParameterMeta>.Select(x => x.ParameterType), item.Type)
            );
        }
    }

    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
    public interface ICommandParameterMeta
    {
        Type ParameterType { get; }
    }
}