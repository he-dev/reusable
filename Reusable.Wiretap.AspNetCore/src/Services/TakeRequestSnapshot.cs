using Microsoft.AspNetCore.Http;

namespace Reusable.Wiretap.Utilities.AspNetCore.Services;

public class TakeRequestSnapshot
{
    public virtual object Invoke(HttpRequest request) => new
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