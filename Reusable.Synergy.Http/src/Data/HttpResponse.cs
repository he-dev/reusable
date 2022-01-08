using System.Net;

namespace Reusable.Synergy.Data;

public class HttpResponse
{
    public HttpStatusCode StatusCode { get; set; }
    
    public HttpStatusCodeClass StatusCodeClass => (HttpStatusCodeClass)((int)StatusCode / 100);

    public object? Body { get; set; }

    public string? ContentType { get; set; }
}