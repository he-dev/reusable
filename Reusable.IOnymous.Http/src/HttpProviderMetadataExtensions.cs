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
        public static MetadataScope<HttpProvider> Http(this Metadata metadata)
        {
            return metadata.For<HttpProvider>();
        }

        public static Metadata Http(this Metadata metadata, ConfigureMetadataScopeCallback<HttpProvider> scope)
        {
            return metadata.For(scope);
        }

        public static Stream Content(this MetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(Stream.Null);
        }

        public static MetadataScope<HttpProvider> Content(this MetadataScope<HttpProvider> scope, Stream content)
        {
            return scope.Metadata.SetItemWithCallerName(content);
        }

        public static Action<HttpRequestHeaders> ConfigureRequestHeaders(this MetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName<Action<HttpRequestHeaders>>(_ => { });
        }

        public static MetadataScope<HttpProvider> ConfigureRequestHeaders(this MetadataScope<HttpProvider> scope, Action<HttpRequestHeaders> configureRequestHeaders)
        {
            var current = scope.ConfigureRequestHeaders();
            return scope.Metadata.SetItemWithCallerName((Action<HttpRequestHeaders>)(headers =>
            {
                current(headers);
                configureRequestHeaders(headers);
            }));
        }

        public static MediaTypeFormatter RequestFormatter(this MetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName<MediaTypeFormatter>(new JsonMediaTypeFormatter());
        }

        public static MetadataScope<HttpProvider> RequestFormatter(this MetadataScope<HttpProvider> scope, MediaTypeFormatter requestFormatter)
        {
            return scope.Metadata.SetItemWithCallerName(requestFormatter);
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

        public static IEnumerable<MediaTypeFormatter> ResponseFormatters(this MetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(new MediaTypeFormatter[] { new JsonMediaTypeFormatter() });
        }

        public static MetadataScope<HttpProvider> ResponseFormatters(this MetadataScope<HttpProvider> scope, params MediaTypeFormatter[] responseFormatters)
        {
            return scope.Metadata.SetItemWithCallerName((IEnumerable<MediaTypeFormatter>)responseFormatters);
        }

        public static Type ResponseType(this MetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(typeof(object));
        }

        public static MetadataScope<HttpProvider> ResponseType(this MetadataScope<HttpProvider> scope, Type responseType)
        {
            return scope.Metadata.SetItemWithCallerName(responseType);
        }

        // ---

        public static string ContentType(this MetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName("application/json");
        }

        public static MetadataScope<HttpProvider> ContentType(this MetadataScope<HttpProvider> scope, string contentType)
        {
            return scope.Metadata.SetItemWithCallerName(contentType);
        }        
    }
}