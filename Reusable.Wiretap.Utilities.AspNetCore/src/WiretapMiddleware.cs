using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Toggle;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Utilities.AspNetCore;

[UsedImplicitly]
[PublicAPI]
public class WiretapMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly WiretapMiddlewareConfiguration _configuration;

    public WiretapMiddleware
    (
        ILogger<WiretapMiddleware> logger,
        RequestDelegate next,
        WiretapMiddlewareConfiguration configuration
    )
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context, IFeatureService features)
    {
        using var unitOfWork = _logger.BeginUnitOfWork(id: _configuration.GetCorrelationId(context));

        var requestBody = default(string);
        if (features.TryUse(Features.LogRequestBody, out var logRequestBody))
        {
            using var s = logRequestBody;
            requestBody = await _configuration.SerializeRequestBody(context);
        }

        _logger.Log(
            Telemetry
                .Collect
                .Application()
                .Metadata(new { HttpRequest = _configuration.TakeRequestSnapshot(context.Request) })
                .Then(e => e.Message(requestBody)));

        try
        {
            var responseBody = default(string);
            var responseBodyOriginal = context.Response.Body;

            await using (var memory = new MemoryStream())
            {
                context.Response.Body = memory;

                await _next(context);

                using (var reader = new StreamReader(memory.Rewind()))
                {
                    if (features.TryUse(Features.LogResponseBody, out var logResponseBody))
                    {
                        using var s = logResponseBody;
                        responseBody = await reader.ReadToEndAsync();
                    }

                    if (_configuration.CanUpdateOriginalResponseBody(context))
                    {
                        // Update the original response-body.
                        await memory.Rewind().CopyToAsync(responseBodyOriginal);
                    }

                    // Restore the original response-body.
                    context.Response.Body = responseBodyOriginal;
                }
            }

            _logger.Log(
                Telemetry
                    .Collect
                    .Application()
                    .Metadata(new { HttpResponse = _configuration.TakeResponseSnapshot(context.Response) })
                    .Then(e => e.Message(responseBody)));
        }
        catch (Exception inner)
        {
            unitOfWork.SetException(inner);
            throw;
        }
    }

    public static class Features
    {
        public static readonly string LogRequestBody = nameof(LogRequestBody);
        public static readonly string LogResponseBody = nameof(LogResponseBody);
    }
}