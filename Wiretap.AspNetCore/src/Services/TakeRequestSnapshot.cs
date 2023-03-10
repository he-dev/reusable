using Microsoft.AspNetCore.Http;
using Reusable.Wiretap.AspNetCore.Abstractions;

namespace Reusable.Wiretap.AspNetCore.Services;

public class TakeRequestSnapshot : ITakeSnapshot<HttpRequest>
{
    public object Invoke(HttpRequest request) => new
    {
        Path = request.Path.Value,
        Host = request.Host.Value,
        request.ContentLength,
        request.ContentType,
        request.Cookies,
        request.Headers,
        request.IsHttps,
        request.Method,
        request.Protocol,
        request.QueryString,
    };
}