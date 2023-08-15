using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Reusable.Wiretap.AspNetCore.Abstractions;

namespace Reusable.Wiretap.AspNetCore.Services;

public class ResponseFilter : IFilter<HttpResponse>
{
    public bool Matches(HttpContext context)
    {
        return Regex.IsMatch(context.Request.ContentType ?? string.Empty, "(text/(plain|html))|(application/json)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}