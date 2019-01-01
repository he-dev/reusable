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
        public static Stream Content(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<Stream>();
        }

        public static ResourceMetadata Content(this ResourceMetadata metadata, Stream content)
        {
            return metadata.SetItemSafe(content);
        }

        public static Action<HttpRequestHeaders> ConfigureRequestHeaders(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<Action<HttpRequestHeaders>>(_ => { });
        }

        public static ResourceMetadata ConfigureRequestHeaders(this ResourceMetadata metadata, Action<HttpRequestHeaders> configureRequestHeaders)
        {
            var current = metadata.ConfigureRequestHeaders();
            return metadata.SetItemSafe((Action<HttpRequestHeaders>)(headers =>
            {
                current(headers);
                configureRequestHeaders(headers);
            }));
        }

        public static MediaTypeFormatter RequestFormatter(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<MediaTypeFormatter>(new JsonMediaTypeFormatter());
        }

        public static ResourceMetadata RequestFormatter(this ResourceMetadata metadata, MediaTypeFormatter requestFormatter)
        {
            return metadata.SetItemSafe(requestFormatter);
        }

        public static bool EnsureSuccessStatusCode(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(true);
        }

        public static ResourceMetadata EnsureSuccessStatusCode(this ResourceMetadata metadata, bool ensureSuccessStatusCode)
        {
            return metadata.SetItemSafe(ensureSuccessStatusCode);
        }

        public static IEnumerable<MediaTypeFormatter> ResponseFormatters(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(new MediaTypeFormatter[] { new JsonMediaTypeFormatter() });
        }

        public static ResourceMetadata ResponseFormatters(this ResourceMetadata metadata, params MediaTypeFormatter[] responseFormatters)
        {
            return metadata.SetItemSafe((IEnumerable<MediaTypeFormatter>)responseFormatters);
        }

        public static Type ResponseType(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(typeof(object));
        }

        public static ResourceMetadata ResponseType(this ResourceMetadata metadata, Type responseType)
        {
            return metadata.SetItemSafe(responseType);
        }

        // ---

        public static string ContentType(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault("application/json");
        }

        public static ResourceMetadata ContentType(this ResourceMetadata metadata, string contentType)
        {
            return metadata.SetItemSafe(contentType);
        }

        // ---

        public static Stream Content(this ResourceMetadataScope<HttpProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Stream.Null);
        }

        public static ResourceMetadataScope<HttpProvider> Content(this ResourceMetadataScope<HttpProvider> scope, Stream content)
        {
            return scope.Metadata.SetItemSafe(content);
        }
    }
}