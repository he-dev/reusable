using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.AspNetCore.Abstractions;
using Reusable.Wiretap.AspNetCore.Services;

namespace Reusable.Wiretap.AspNetCore;

[UsedImplicitly]
[PublicAPI]
public class WiretapMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly ITakeSnapshot<HttpRequest> _takeRequestSnapshot;
    private readonly ITakeSnapshot<HttpResponse> _takeResponseSnapshot;
    private readonly IFilter<HttpRequest> _requestFilter;
    private readonly IFilter<HttpResponse> _responseFilter;

    public WiretapMiddleware
    (
        ILogger<WiretapMiddleware> logger,
        RequestDelegate next,
        ITakeSnapshot<HttpRequest> takeRequestSnapshot,
        ITakeSnapshot<HttpResponse> takeResponseSnapshot,
        IFilter<HttpRequest> requestFilter,
        IFilter<HttpResponse> responseFilter
    )
    {
        _logger = logger;
        _next = next;
        _takeRequestSnapshot = takeRequestSnapshot;
        _takeResponseSnapshot = takeResponseSnapshot;
        _requestFilter = requestFilter;
        _responseFilter = responseFilter;
    }

    public async Task Invoke(HttpContext context)
    {
        using (var activity = _logger.Begin("DumpRequest", details: new { context.TraceIdentifier }))
        {
            var requestBody = default(object);
            if (_requestFilter.Matches(context))
            {
                context.Request.EnableBuffering();
                if (context.Request.Body.TryRewind())
                {
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                }
            }

            activity.LogRequest(details: new { request = _takeRequestSnapshot.Invoke(context.Request) }, attachment: requestBody);
        }

        // save the original stream for later to restore it
        var responseStream = context.Response.Body;

        // replace the stream with one supporting seeking
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        using (var activity = _logger.Begin("MeasureRequest", details: new { context.TraceIdentifier }))
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                activity.LogError(attachment: e);
                throw;
            }
        }

        using (var activity = _logger.Begin("DumpResponse", details: new { context.TraceIdentifier }))
        {
            var responseBody = default(object);
            try
            {
                if (_responseFilter.Matches(context))
                {
                    if (context.Response.Body.TryRewind())
                    {
                        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
                        responseBody = await reader.ReadToEndAsync();
                        // copy the response back to the original stream
                        await memoryStream.Rewind().CopyToAsync(responseStream);
                    }
                }
            }
            catch (Exception e)
            {
                activity.LogError(attachment: e);
                throw;
            }
            finally
            {
                // restore the original stream
                context.Response.Body = responseStream;
            }

            activity.LogResponse(details: new { response = _takeResponseSnapshot.Invoke(context.Response) }, attachment: responseBody);
        }
    }
}