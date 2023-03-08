using System;
using System.IO;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Utilities.AspNetCore.Extensions;
using Reusable.Wiretap.Utilities.AspNetCore.Mvc.Filters;
using Reusable.Wiretap.Utilities.AspNetCore.Services;

namespace Reusable.Wiretap.Utilities.AspNetCore;

[UsedImplicitly]
[PublicAPI]
public class WiretapMiddleware
{
    private readonly ILogger _taskLogger;
    private readonly RequestDelegate _next;
    private readonly TakeRequestSnapshot _takeRequestSnapshot;
    private readonly TakeResponseSnapshot _takeResponseSnapshot;
    private readonly SerializeRequestBody _serializeRequestBody;
    private readonly Configuration _configuration;

    public WiretapMiddleware
    (
        ILogger taskLogger,
        RequestDelegate next,
        TakeRequestSnapshot takeRequestSnapshot,
        TakeResponseSnapshot takeResponseSnapshot,
        SerializeRequestBody serializeRequestBody,
        Configuration configuration
    )
    {
        _taskLogger = taskLogger;
        _next = next;
        _takeRequestSnapshot = takeRequestSnapshot;
        _takeResponseSnapshot = takeResponseSnapshot;
        _serializeRequestBody = serializeRequestBody;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        using var requestFlow = _taskLogger.Start(_configuration.GetCorrelationId(context));

        var requestBody =
            _configuration.CanLogRequestBody(context)
                ? await _serializeRequestBody.Invoke(context)
                : default;

        requestFlow.Running(new { HttpRequest = _takeRequestSnapshot.Invoke(context.Request) }, requestBody);

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
                    if (context.Items.TryGetValue(nameof(CanLogResponseBody), out var item) && item is true)
                    {
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

            requestFlow.Running(new { HttpResponse = _takeResponseSnapshot.Invoke(context.Response) }, responseBody);
        }
        catch (Exception inner)
        {
            requestFlow.Faulted(attachment: inner);
            throw;
        }
    }

    [PublicAPI]
    public class Configuration
    {
        public Func<HttpContext, string> GetCorrelationId { get; set; } = context => context.GetCorrelationId() ?? context.TraceIdentifier;

        public Func<HttpContext, object> GetCorrelationHandle { get; set; } = _ => "HttpRequest";

        public Func<HttpContext, bool> CanLogRequestBody { get; set; } = _ => true;

        public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;

        public Func<HttpContext, bool> CanUpdateOriginalResponseBody { get; set; } = c => !c.Response.StatusCode.In(204, 304);
    }
}