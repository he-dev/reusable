using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public static class ResourceMetadataExtensions
    {
        public static Stream Content(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Stream.Null);
        }

        public static ResourceMetadataScope<HttpProvider> Content(this ResourceMetadataScope<HttpProvider> scope, Stream content)
        {
            return scope.Metadata.SetItemSafe(content);
        }

        public static Action<HttpRequestHeaders> ConfigureRequestHeaders(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault<Action<HttpRequestHeaders>>(_ => { });
        }

        public static ResourceMetadataScope<HttpProvider> ConfigureRequestHeaders(this ResourceMetadataScope<HttpProvider> scope, Action<HttpRequestHeaders> configureRequestHeaders)
        {
            var current = scope.ConfigureRequestHeaders();
            return scope.Metadata.SetItemSafe((Action<HttpRequestHeaders>)(headers =>
            {
                current(headers);
                configureRequestHeaders(headers);
            }));
        }

        public static MediaTypeFormatter RequestFormatter(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault<MediaTypeFormatter>(new JsonMediaTypeFormatter());
        }

        public static ResourceMetadataScope<HttpProvider> RequestFormatter(this ResourceMetadataScope<HttpProvider> scope, MediaTypeFormatter requestFormatter)
        {
            return scope.Metadata.SetItemSafe(requestFormatter);
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

        public static IEnumerable<MediaTypeFormatter> ResponseFormatters(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(new MediaTypeFormatter[] { new JsonMediaTypeFormatter() });
        }

        public static ResourceMetadataScope<HttpProvider> ResponseFormatters(this ResourceMetadataScope<HttpProvider> scope, params MediaTypeFormatter[] responseFormatters)
        {
            return scope.Metadata.SetItemSafe((IEnumerable<MediaTypeFormatter>)responseFormatters);
        }

        public static Type ResponseType(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(typeof(object));
        }

        public static ResourceMetadataScope<HttpProvider> ResponseType(this ResourceMetadataScope<HttpProvider> scope, Type responseType)
        {
            return scope.Metadata.SetItemSafe(responseType);
        }

        // ---

        public static string ContentType(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault("application/json");
        }

        public static ResourceMetadataScope<HttpProvider> ContentType(this ResourceMetadataScope<HttpProvider> scope, string contentType)
        {
            return scope.Metadata.SetItemSafe(contentType);
        }        
    }
}