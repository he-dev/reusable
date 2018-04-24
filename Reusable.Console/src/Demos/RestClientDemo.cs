using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Net;

namespace Reusable.Apps.Demos
{
    public static class RestClientDemo
    {
        public static void Start()
        {
            var configureDefaultRequestHeaders =
                new HttpRequestHeadersConfiguration()
                    .Clear()
                    .AcceptJson()
                    .AddRange(new Dictionary<string, IEnumerable<string>>
                    {
                        ["X-Reusable-Environment"] = new[] { "development" },
                        ["X-Reusable-Product"] = new[] { "Reusable" },
                        ["X-Reusable-ProductVersion"] = new[] { "4.0.0" }
                    });

            var client = new RestClient("http://localhost:54245/api/", configureDefaultRequestHeaders);


        }
    }

    public interface ITests { }

    public static class TestsClient
    {
        public static IResourceContext<ITests> Tests(this IRestClient client)
        {
            return client.ToResource<ITests>();
        }
    }

    public static class TestClientExtensions
    {
        public static Task MessageAsync(this IResourceContext<ITests> resourceContext)
        {
            return resourceContext.Client.GetAsync<string>(resourceContext.UriDynamicPart, resourceContext.MethodContext, CancellationToken.None);
        }
    }
}
