using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public static class HttpProviderMetadataExtensions
    {
        public static Metadata<HttpProvider> Http(this Metadata metadata)
        {
            return metadata.Scope<HttpProvider>();
        }

        public static Metadata Http(this Metadata metadata, ConfigureMetadataScopeCallback<HttpProvider> scope)
        {
            return metadata.Scope(scope);
        }

        public static Stream Content(this Metadata<HttpProvider> scope)
        {
            return scope.Value.GetItemByCallerName(Stream.Null);
        }

        public static Metadata<HttpProvider> Content(this Metadata<HttpProvider> scope, Stream content)
        {
            return scope.Value.SetItemByCallerName(content);
        }

        public static Action<HttpRequestHeaders> ConfigureRequestHeaders(this Metadata<HttpProvider> scope)
        {
            return scope.Value.GetItemByCallerName<Action<HttpRequestHeaders>>(_ => { });
        }

        public static Metadata<HttpProvider> ConfigureRequestHeaders(this Metadata<HttpProvider> scope, Action<HttpRequestHeaders> configureRequestHeaders)
        {
            var current = scope.ConfigureRequestHeaders();
            return scope.Value.SetItemByCallerName((Action<HttpRequestHeaders>)(headers =>
            {
                current(headers);
                configureRequestHeaders(headers);
            }));
        }

        public static MediaTypeFormatter RequestFormatter(this Metadata<HttpProvider> scope)
        {
            return scope.Value.GetItemByCallerName<MediaTypeFormatter>(new JsonMediaTypeFormatter());
        }

        public static Metadata<HttpProvider> RequestFormatter(this Metadata<HttpProvider> scope, MediaTypeFormatter requestFormatter)
        {
            return scope.Value.SetItemByCallerName(requestFormatter);
        }

        // public static bool EnsureSuccessStatusCode(this ResourceMetadata metadata)
        // {
        //     return metadata.GetValueOrDefault(true);
        // }
        //
        // public static ResourceMetadata EnsureSuccessStatusCode(this ResourceMetadata metadata, bool ensureSuccessStatusCode)
        // {
        //     return metadata.SetItemSafe(ensureSuccessStatusCode);
        // }

        public static IEnumerable<MediaTypeFormatter> ResponseFormatters(this Metadata<HttpProvider> scope)
        {
            return scope.Value.GetItemByCallerName(new MediaTypeFormatter[] { new JsonMediaTypeFormatter() });
        }

        public static Metadata<HttpProvider> ResponseFormatters(this Metadata<HttpProvider> scope, params MediaTypeFormatter[] responseFormatters)
        {
            return scope.Value.SetItemByCallerName((IEnumerable<MediaTypeFormatter>)responseFormatters);
        }

        public static Type ResponseType(this Metadata<HttpProvider> scope)
        {
            return scope.Value.GetItemByCallerName(typeof(object));
        }

        public static Metadata<HttpProvider> ResponseType(this Metadata<HttpProvider> scope, Type responseType)
        {
            return scope.Value.SetItemByCallerName(responseType);
        }

        // ---

        public static string ContentType(this Metadata<HttpProvider> scope)
        {
            return scope.Value.GetItemByCallerName("application/json");
        }

        public static Metadata<HttpProvider> ContentType(this Metadata<HttpProvider> scope, string contentType)
        {
            return scope.Value.SetItemByCallerName(contentType);
        }        
    }
}