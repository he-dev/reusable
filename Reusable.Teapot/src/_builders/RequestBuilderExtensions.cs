using System;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;

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
                if (request.CanShortCircuit())
                {
                    if (++counter > exactly)
                    {
                        throw DynamicException.Create(nameof(Occurs), $"Api was called {counter} time(s) but expected {exactly}.");
                    }
                }
                else
                {
                    if (counter != exactly)
                    {
                        throw DynamicException.Create(nameof(Occurs), $"Api was called {counter} time(s) but expected {exactly}.");
                    }
                }
            }, true);
        }

        public static IRequestBuilder WithHeader(this IRequestBuilder builder, string header, params string[] expectedValues)
        {
            return builder.Add(request =>
            {
                if (request.CanShortCircuit())
                {
                    if (request.Headers.TryGetValue(header, out var actualValues))
                    {
                        if (actualValues.Intersect(expectedValues).Count() != expectedValues.Count())
                        {
                            throw DynamicException.Create
                            (
                                "DifferentHeader",
                                $"Expected: [{expectedValues.Join(", ")}]{Environment.NewLine}" +
                                $"Actual:   [{actualValues.Join(", ")}]"
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
                }
            }, false);
        }

        public static IRequestBuilder WithApiVersion(this IRequestBuilder builder, string version) => builder.WithHeader("Api-Version", version);

        public static IRequestBuilder WithContentType(this IRequestBuilder builder, string mediaType) => builder.WithHeader("Content-Type", mediaType);

        public static IRequestBuilder WithContentTypeJson(this IRequestBuilder builder, Action<ContentSection<JToken>> contentAssert)
        {
            return builder.Add(request =>
            {
                var content = request.DeserializeAsJToken();
                contentAssert(ContentSection.FromJToken(content));
            }, false);
        }

        public static IRequestBuilder AsUserAgent(this IRequestBuilder builder, string product, string version) => builder.WithHeader("User-Agent", $"{product}/{version}");

        public static IRequestBuilder Accepts(this IRequestBuilder builder, string mediaType) => builder.WithHeader("Accept", mediaType);

        public static IRequestBuilder AcceptsJson(this IRequestBuilder builder) => builder.Accepts("application/json");

        public static IRequestBuilder AcceptsHtml(this IRequestBuilder builder) => builder.Accepts("text/html");
    }

    
}