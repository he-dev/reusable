using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    public static class SettingRequestFactory
    {
        public static (UriString Uri, Metadata Metadata) CreateSettingRequest(SettingMetadata setting, string handle = null)
        {
            var queryParameters = new (SoftString Key, SoftString Value)[]
            {
                //(ResourceQueryStringKeys.ProviderName, setting.ResourceProviderName),
                //(ResourceQueryStringKeys.ProviderType, setting.ResourceProviderType?.ToPrettyString()),
                (SettingQueryStringKeys.Prefix, setting.ResourcePrefix),
                (SettingQueryStringKeys.Handle, handle),
                (SettingQueryStringKeys.Level, setting.Level.ToString()),
                (SettingQueryStringKeys.IsCollection, typeof(IList<string>).IsAssignableFrom(setting.MemberType).ToString()),
            };

            var query =
                queryParameters
                    .Where(x => x.Value)
                    .Select(x => $"{x.Key.ToString()}={x.Value.ToString()}")
                    .Join("&");

            return
            (
                $"{setting.ResourceScheme}:///{setting.Scope.Replace('.', '/')}/{setting.TypeName}/{setting.MemberName}{((SoftString)query ? $"?{query}" : string.Empty)}",
                Metadata.Empty.Provider(s => s.CustomName(setting.ResourceProviderName).DefaultName(setting.ResourceProviderType?.ToPrettyString()))
            );
        }
    }
}