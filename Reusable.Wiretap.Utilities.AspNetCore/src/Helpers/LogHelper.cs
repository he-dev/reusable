using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reusable.Marbles.Extensions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Utilities.AspNetCore.Helpers;

public static class LogHelper
{
    public static object TakeRequestSnapshot(HttpRequest request)
    {
        return new
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

    public static async Task<string?> DumpRequestBody(this HttpContext context)
    {
        if (context.Request.ContentLength > 0)
        {
            try
            {
                await using var requestCopy = new MemoryStream();
                using var requestReader = new StreamReader(requestCopy);
                context.Request.EnableBuffering();
                await context.Request.Body.CopyToAsync(requestCopy);
                requestCopy.Rewind();
                return await requestReader.ReadToEndAsync();
            }
            finally
            {
                context.Request.Body.Rewind();
            }
        }
        else
        {
            return default;
        }
    }

    public static object TakeResponseSnapshot(HttpResponse response)
    {
        return new
        {
            response.ContentLength,
            response.ContentType,
            response.Headers,
            response.StatusCode,
        };
    }
}