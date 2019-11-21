using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryExtensions
    {
        public static Task<Response> HttpInvokeAsync
        (
            this IResourceRepository resources,
            Option<RequestMethod> method,
            UriString uri,
            object? body = default,
            Action<HttpRequest>? requestAction = default
        )
        {
            return resources.InvokeAsync(Request.Create(method, uri, body, requestAction));
        }
    }

    [Scheme("http", "https")]
    public class HttpRequest : Request
    {
        public ProductInfoHeaderValue? UserAgent { get; set; }

        public Action<HttpRequestHeaders>? ConfigureHeaders { get; set; }

        public MediaTypeFormatter? RequestFormatter { get; set; }

        public string ContentType { get; set; } = "application/json";
    }

    public class HttpResponseOptions
    {
        //public static IEnumerable<MediaTypeFormatter> Formatters { get; set; }

        public static Type ResponseType { get; set; }

        public static string ContentType { get; set; }
    }
}