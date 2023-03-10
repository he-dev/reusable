using Microsoft.AspNetCore.Http;
using Reusable.Wiretap.AspNetCore.Abstractions;

namespace Reusable.Wiretap.AspNetCore.Services;

public class TakeResponseSnapshot : ITakeSnapshot<HttpResponse>
{
    public object Invoke(HttpResponse response) => new
    {
        response.ContentLength,
        response.ContentType,
        response.Headers,
        response.StatusCode,
    };
}