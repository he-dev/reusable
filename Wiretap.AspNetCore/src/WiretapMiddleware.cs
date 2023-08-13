using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.AspNetCore.Abstractions;
using Reusable.Wiretap.AspNetCore.Extensions;

namespace Reusable.Wiretap.AspNetCore;

[UsedImplicitly]
[PublicAPI]
public class WiretapMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly ITakeSnapshot<HttpRequest> _takeRequestSnapshot;
    private readonly ITakeSnapshot<HttpResponse> _takeResponseSnapshot;
    private readonly ISerialize<HttpRequest> _serializeRequest;
    private readonly ISerialize<HttpResponse> _serializeResponse;
    private readonly Configuration _configuration;

    public WiretapMiddleware
    (
        ILogger logger,
        RequestDelegate next,
        ITakeSnapshot<HttpRequest> takeRequestSnapshot,
        ITakeSnapshot<HttpResponse> takeResponseSnapshot,
        ISerialize<HttpRequest> serializeRequest,
        ISerialize<HttpResponse> serializeResponse,
        Configuration configuration
    )
    {
        _logger = logger;
        _next = next;
        _takeRequestSnapshot = takeRequestSnapshot;
        _takeResponseSnapshot = takeResponseSnapshot;
        _serializeRequest = serializeRequest;
        _serializeResponse = serializeResponse;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = _configuration.GetCorrelationId(context);
        var requestBody = _configuration.CanLogRequestBody(context) ? await _serializeRequest.Invoke(context.Request) : default;
        using var activity = _logger.LogBegin("HandleRequest", details: new { correlationId, request = _takeRequestSnapshot.Invoke(context.Request) }, attachment: requestBody);

        try
        {
            await _next(context);
            var responseBody = _configuration.CanLogResponseBody(context) ? await _serializeResponse.Invoke(context.Response) : default;
            activity.LogEnd(details: new { correlationId, response = _takeResponseSnapshot.Invoke(context.Response) }, attachment: responseBody);
        }
        catch (Exception inner)
        {
            activity.LogError(details: new { correlationId, response = _takeResponseSnapshot.Invoke(context.Response) }, attachment: inner);
            throw;
        }
    }

    [PublicAPI]
    public class Configuration
    {
        public Func<HttpContext, string> GetCorrelationId { get; set; } = context => context.CorrelationId() ?? context.TraceIdentifier;

        public Func<HttpContext, bool> CanLogRequestBody { get; set; } = _ => true;

        public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;

        //public Func<HttpContext, bool> CanUpdateOriginalResponseBody { get; set; } = c => !c.Response.StatusCode.In(204, 304);
    }
}