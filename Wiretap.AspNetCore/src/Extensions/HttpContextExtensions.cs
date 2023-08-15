using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Reusable.Wiretap.AspNetCore.Extensions;

public static class HttpContextExtensions
{
    public static string? CorrelationId(this IHeaderDictionary headers, string header = "X-Correlation-ID")
    {
        return headers[header] switch
        {
            { Count: 1 } correlationId => correlationId.Single(), _ => default
        };
    }
}