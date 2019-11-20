using System.Net.Http;
using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class HttpControllerExtensions
    {
        public static IResourceCollection AddHttp(this IResourceCollection controllers, string baseUri, IImmutableContainer? properties = default)
        {
            return controllers.Add(HttpController.FromBaseUri(baseUri, properties));
        }

        public static IResourceCollection AddHttp(this IResourceCollection controllers, HttpClient httpClient, IImmutableContainer? properties = default)
        {
            return controllers.Add(new HttpController(httpClient, properties));
        }
    }
}