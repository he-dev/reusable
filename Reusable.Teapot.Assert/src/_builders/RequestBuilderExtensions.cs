using System;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Marbles;

namespace Reusable.Teapot
{
    [PublicAPI]
    public static class RequestBuilderExtensions
    {
        public static IRequestAssert Occurs(this IRequestAssert assert, int exactly)
        {
            var counter = 0;
            return assert.Add(request =>
            {
                if (request is {})
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
            });
        }

        public static IRequestAssert WithHeader(this IRequestAssert assert, string header, params string[] expectedValues)
        {
            return assert.Add(request =>
            {
                if (request is {})
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
            });
        }

        public static IRequestAssert ApiVersion(this IRequestAssert assert, string version) => assert.WithHeader("Api-Version", version);

        public static IRequestAssert WithContentType(this IRequestAssert assert, string mediaType) => assert.WithHeader("Content-Type", mediaType);

        public static IRequestAssert ContentTypeJsonWhere(this IRequestAssert assert, Action<ContentSection<JToken>> contentAssert)
        {
            return assert.Add(request =>
            {
                if (request is {})
                {
                    var content = request.DeserializeAsJToken();
                    contentAssert(ContentSection.FromJToken(content));
                }
            });
        }

        public static IRequestAssert UserAgent(this IRequestAssert assert, string product, string version) => assert.WithHeader("User-Agent", $"{product}/{version}");

        public static IRequestAssert Accepts(this IRequestAssert assert, string mediaType) => assert.WithHeader("Accept", mediaType);

        public static IRequestAssert AcceptsJson(this IRequestAssert assert) => assert.Accepts("application/json");

        public static IRequestAssert AcceptsHtml(this IRequestAssert assert) => assert.Accepts("text/html");
    }

    
}