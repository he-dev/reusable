using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reusable.Essentials.Extensions;
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

    public static async Task<string?> SerializeRequestBody(this HttpContext context)
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

    /// <summary>
    /// Maps http-status-code to OmiLog log-level.
    /// </summary>
    public static Func<int, LogLevel> MapStatusCode { get; set; } = statusCode =>
    {
        return statusCode switch
        {
            var x when x >= 500 => LogLevel.Fatal,
            var x when x >= 400 => LogLevel.Error,
            var x when x >= 300 => LogLevel.Warning,
            _ => LogLevel.Information,
        };
    };
}