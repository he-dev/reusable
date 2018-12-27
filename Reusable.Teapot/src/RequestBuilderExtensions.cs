using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionizer;

namespace Reusable.Teapot
{
    [PublicAPI]
    public static class RequestBuilderExtensions
    {
        public static IRequestBuilder Occurs(this IRequestBuilder builder, int exactly)
        {
            var counter = 0;
            return builder.Add(request =>
            {
                if (request is null)
                {
                    if (counter != exactly)
                    {
                        throw DynamicException.Create(nameof(Occurs), $"Resource {builder.Uri} was requested {counter} time(s) but expected {exactly}.");
                    }
                }
                else
                {
                    if (++counter > exactly)
                    {
                        throw DynamicException.Create(nameof(Occurs), $"Resource {builder.Uri} was requested {counter} time(s) but expected {exactly}.");
                    }
                }
            }, true);
        }

        public static IRequestBuilder WithHeader(this IRequestBuilder builder, string header, params string[] expectedValue)
        {
            //var expectedHeader = Regex.Replace(header, @"\-", string.Empty);

            return builder.Add(request =>
            {
                if (request.Headers.TryGetValue(header, out var values))
                {
                    if (values.Intersect(expectedValue).Count() != expectedValue.Count())
                    {
                        throw DynamicException.Create
                        (
                            "DifferentHeaderValue",
                            $"Header '{header}' has different values."
                        );
                    }
                }
                else
                {
                    throw DynamicException.Create
                    (
                        "HeaderNotFound",
                        $"Header '{header}' is missing."
                    );
                }
            }, false);
        }

        public static IRequestBuilder WithApiVersion(this IRequestBuilder builder, string version) => builder.WithHeader("Api-Version", version);

        public static IRequestBuilder WithContentType(this IRequestBuilder builder, string mediaType) => builder.WithHeader("Content-Type", mediaType);

        public static IRequestBuilder WithContentTypeJson(this IRequestBuilder builder, Action<IContentAssert<JToken>> contentAssert)
        {
            //assert.WithHeader("Content-Type", "application/json; charset=utf-8");

            return builder.Add(request =>
            {
                var content = request.DeserializeAsJToken();
                contentAssert(new ContentAssert<JToken>(content));
            }, false);
        }

        public static IRequestBuilder AsUserAgent(this IRequestBuilder builder, string product, string version) => builder.WithHeader("User-Agent", $"{product}/{version}");

        public static IRequestBuilder Accepts(this IRequestBuilder builder, string mediaType) => builder.WithHeader("Accept", mediaType);

        public static IRequestBuilder AcceptsJson(this IRequestBuilder builder) => builder.Accepts("application/json");
    }

    public interface IContentAssert<out TContent>
    {
        [CanBeNull]
        TContent Value { get; }
    }

    internal class ContentAssert<TContent> : IContentAssert<TContent>
    {
        public ContentAssert(TContent value) => Value = value;

        public TContent Value { get; }
    }


    public static class ContentAssertExtensions
    {
        public static IContentAssert<JToken> HasProperty(this IContentAssert<JToken> content, string jsonPath)
        {
            if (content.Value is null)
            {
                throw DynamicException.Create("ContentNull", "There is no content.");
            }

            return
                content.Value.SelectToken(jsonPath) is null
                    ? throw DynamicException.Create("ContentPropertyNotFound", $"There is no such property as '{jsonPath}'")
                    : content;
        }
    }
}