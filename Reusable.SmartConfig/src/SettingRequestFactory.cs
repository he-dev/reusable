using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    public static class SettingRequestFactory
    {
        public static (UriString Uri, IImmutableSession Metadata) CreateSettingRequest(SettingMetadata setting, string handle = null)
        {
            var queryParameters = new (SoftString Key, SoftString Value)[]
            {
                (SettingQueryStringKeys.Prefix, setting.ResourcePrefix),
                (SettingQueryStringKeys.Handle, handle),
                (SettingQueryStringKeys.Level, setting.Level.ToString()),
                (SettingQueryStringKeys.IsCollection, typeof(IList<string>).IsAssignableFrom(setting.MemberType).ToString()),
            };

            var path = new[]
            {
                setting.Scope.Replace('.', '/'),
                setting.TypeName,
                setting.MemberName
            }.Join("/");

            var query =
                queryParameters
                    .Where(x => x.Value)
                    .Select(x => $"{x.Key.ToString()}={x.Value.ToString()}")
                    .Join("&");

            query = (SoftString)query ? $"?{query}" : string.Empty;

            return
            (
                $"{setting.ResourceScheme}:///{path}{query}",
                ImmutableSession.Empty.Scope<IProviderSession>(s => s
                    .Set(x => x.CustomName, setting.ResourceProviderName)
                    .Set(x => x.DefaultName, setting.ResourceProviderType?.ToPrettyString()))
            );
        }
    }
}