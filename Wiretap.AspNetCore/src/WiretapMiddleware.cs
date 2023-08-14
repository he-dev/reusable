using System;
using System.IO;
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
        ILogger<WiretapMiddleware> logger,
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
        using var activity = _logger.Begin("HandleRequest");
        using var detailsToken = activity.Items.PushDetails(new { correlationId });
        activity.LogRequest(details: new { request = _takeRequestSnapshot.Invoke(context.Request) }, attachment: requestBody);

        var responseStream = context.Response.Body;
        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            var responseBody = _configuration.CanLogResponseBody(context) ? await _serializeResponse.Invoke(context.Response) : default;

            await memoryStream.CopyToAsync(responseStream);

            activity.LogResponse(details: new { response = _takeResponseSnapshot.Invoke(context.Response) }, attachment: responseBody);
            activity.LogEnd();
        }
        catch (Exception inner)
        {
            activity.LogError(attachment: inner);
            throw;
        }
        finally
        {
            context.Response.Body = responseStream;
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