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
    private readonly SemanticLoggerConfig _config;

    public WiretapMiddleware
    (
        ILoggerFactory loggerFactory,
        RequestDelegate next,
        SemanticLoggerConfig config
    )
    {
        _next = next;
        _config = config;
        _logger = loggerFactory.CreateLogger<WiretapMiddleware>();
    }

    public async Task Invoke(HttpContext context, IFeatureService features)
    {
        using var scope =
            _logger
                .BeginScope()
                .WithId(_config.GetCorrelationId(context))
                .WithName(_config.GetCorrelationHandle(context));

        var requestBody =
            _config.CanLogRequestBody(context)
                ? await _config.SerializeRequestBody(context)
                : default;

        _logger.Log(
            Telemetry
                .Collect
                .Application()
                .Metadata(new { HttpRequest = context.Request })
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

                    if (_config.CanUpdateOriginalResponseBody(context))
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
                    .Metadata(new { HttpResponse = _config.TakeResponseSnapshot(context) })
                    .Then(e => e.Message(responseBody))
                    .Level(_config.MapStatusCode(context.Response.StatusCode)));
        }
        catch (Exception inner)
        {
            scope.Exception(inner);
            throw;
        }
        finally
        {
            _logger.Log(Telemetry.Collect.Application().UnitOfWork(nameof(Invoke)).Auto());
        }
    }

    public static class Features
    {
        public static readonly IFeatureIdentifier LogResponseBody = new FeatureName(nameof(LogResponseBody));
    }
}