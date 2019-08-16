using System.Net.Http;
using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidBuilderExtensions
    {
        public static ResourceSquidBuilder UseHttp(this ResourceSquidBuilder builder, string baseUri, IImmutableContainer properties = default)
        {
            return builder.UseController(HttpController.FromBaseUri(baseUri, properties));
        }

        public static ResourceSquidBuilder UseHttp(this ResourceSquidBuilder builder, HttpClient httpClient, IImmutableContainer properties = default)
        {
            return builder.UseController(new HttpController(httpClient, properties));
        }
    }
}