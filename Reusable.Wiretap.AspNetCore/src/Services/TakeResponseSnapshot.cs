using Microsoft.AspNetCore.Http;

namespace Reusable.Wiretap.Utilities.AspNetCore.Services;

public class TakeResponseSnapshot
{
    public virtual object Invoke(HttpResponse response) => new
    {
        response.ContentLength,
        response.ContentType,
        response.Headers,
        response.StatusCode,
    };
}