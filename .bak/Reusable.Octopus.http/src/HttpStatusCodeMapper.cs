using System.Net;

namespace Reusable.Translucent
{
    public static class HttpStatusCodeMapper
    {
        public static HttpStatusCodeClass MapStatusCode(int statusCode) => (HttpStatusCodeClass)(statusCode / 100);

        public static HttpStatusCodeClass Class(this HttpStatusCode statusCode) => (HttpStatusCodeClass)((int)statusCode / 100);
    }
}