using JetBrains.Annotations;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    [PublicAPI]
    public class HttpResponse : Response
    {
        public int HttpStatusCode { get; set; }

        public HttpStatusCodeClass HttpStatusCodeClass => HttpStatusCodeMapper.MapStatusCode(HttpStatusCode);

        public string? ContentType { get; set; }
    }
}