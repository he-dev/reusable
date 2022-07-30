using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
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

        var requestBody =
            _configuration.CanLogRequestBody(context)
                ? await _configuration.SerializeRequestBody(context)
                : default;

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
                    responseBody = await features.Use(Features.LogResponseBody, async () => await reader.ReadToEndAsync());

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
                    .Then(e => e.Message(responseBody))
                    .Level(_configuration.MapStatusCode(context.Response.StatusCode)));
        }
        catch (Exception inner)
        {
            unitOfWork.SetException(inner);
            throw;
        }
    }

    public static class Features
    {
        public static readonly IFeatureIdentifier LogResponseBody = new FeatureName(nameof(LogResponseBody));
    }
}