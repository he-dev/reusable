using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Essentials.Extensions;
using Reusable.Jumble;
using Reusable.OmniLog.Utilities.AspNetCore;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Utilities.AspNetCore;

[UsedImplicitly]
[PublicAPI]
public class SemanticLogger
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly SemanticLoggerConfig _config;

    public SemanticLogger
    (
        ILoggerFactory loggerFactory,
        RequestDelegate next,
        SemanticLoggerConfig config
    )
    {
        _next = next;
        _config = config;
        _logger = loggerFactory.CreateLogger<SemanticLogger>();
    }

    public async Task Invoke(HttpContext context, IFeatureService features)
    {
        using var scope = 
            _logger
                .BeginScope(nameof(HttpRequest))
                .WithCorrelationId(_config.GetCorrelationId(context))
                .WithCorrelationHandle(_config.GetCorrelationHandle(context));
            
        var requestBody =
            _config.CanLogRequestBody(context)
                ? await _config.SerializeRequestBody(context)
                : default;

        _logger.Log(
            Telemetry
                .Collect
                .Application()
                .Metadata(nameof(HttpRequest), context.Request)
                .Message(requestBody));

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
                    .Metadata(nameof(HttpResponse), _config.TakeResponseSnapshot(context))
                    .Message(responseBody)
                    .Level(_config.MapStatusCode(context.Response.StatusCode)));
        }
        catch (Exception inner)
        {
            _logger.Log(Telemetry.Collect.Application().Execution().Faulted(inner));
            throw;
        }
    }

    public static class Features
    {
        public const string LogResponseBody = nameof(LogResponseBody);
    }
}