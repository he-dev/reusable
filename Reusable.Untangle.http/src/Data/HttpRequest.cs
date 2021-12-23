using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Reusable.Translucent.Data;

public abstract class HttpRequest : Request
{
    public ProductInfoHeaderValue? UserAgent { get; set; }

    public List<Action<HttpRequestHeaders>> HeaderActions { get; } = new();

    //public MediaTypeFormatter? RequestFormatter { get; set; }

    public string ContentType { get; set; } = default!;

    public class Json : HttpRequest
    {
        public Json()
        {
            HeaderActions.Add(headers => headers.AcceptJson());
            ContentType = "application/json";
        }
    }

    public class Stream : HttpRequest { }
}

public static class HttpRequestExtensions
{
    public static void CorrelationId(this HttpRequest request, string correlationId)
    {
        request.HeaderActions.Add(headers => headers.Add("X-Correlation-ID", correlationId));
    }
}