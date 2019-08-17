using System.Net.Http;
using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class HttpControllerExtensions
    {
        public static IResourceControllerBuilder AddHttp(this IResourceControllerBuilder builder, string baseUri, IImmutableContainer properties = default)
        {
            return builder.AddController(HttpController.FromBaseUri(baseUri, properties));
        }

        public static IResourceControllerBuilder AddHttp(this IResourceControllerBuilder builder, HttpClient httpClient, IImmutableContainer properties = default)
        {
            return builder.AddController(new HttpController(httpClient, properties));
        }
    }
}