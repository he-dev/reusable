using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Net;
using Reusable.Net.Http;
using Reusable.Net.Http.Formatting;

namespace Reusable.Apps.Demos
{
    public static class RestClientDemo
    {
        public static void Start()
        {
            var client = new RestClient("http://localhost:49471/api/", headers =>
            {
                headers.AcceptJson();
                headers.UserAgent(productName: "Reusable", productVersion: "7.0");
            });

            var context = new HttpMethodContext(HttpMethod.Get, "mailr", "messages", "test")
            {
                ResponseFormatters = { new TextMediaTypeFormatter() }
            };
            context.RequestHeadersActions.Add(headers => headers.AcceptHtml());
            var result = client.InvokeAsync<string>(context).GetAwaiter().GetResult();

            //var client = new RestClient("http://localhost:54245/api/", configureDefaultRequestHeaders);
        }
    }

    public interface ITests { }

    public static class TestsClient
    {
        public static IResource<ITests> Tests(this IRestClient client, string name) => client.Resource<ITests>(name);
    }

    public static class TestClientExtensions
    {
        public static Task MessageAsync(this IResource<ITests> resource)
        {
            return resource.GetAsync<string>(CancellationToken.None);
        }
    }
}
