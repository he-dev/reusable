using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Reusable.Exceptionize;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public delegate void RequestAssertDelegate(RequestInfo request);

    [CanBeNull]
    public delegate Func<HttpRequest, ResponseInfo> ResponseDelegate(UriString path, SoftString method);

    [UsedImplicitly]
    internal class TeapotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestAssertDelegate _requestAssert;
        private readonly ResponseDelegate _nextResponse;

        public TeapotMiddleware(RequestDelegate next, RequestAssertDelegate requestAssert, ResponseDelegate nextResponse)
        {
            _next = next;
            _requestAssert = requestAssert;
            _nextResponse = nextResponse;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // We'll need this later to restore the body so don't dispose.
                var bodyBackup = new MemoryStream();

                // It needs to be copied because the original stream does not support seeking.
                await context.Request.Body.CopyToAsync(bodyBackup);

                // Each log gets it's own request copy. It's easier to dispose them later.
                var bodyCopy = new MemoryStream();
                await bodyBackup.Rewind().CopyToAsync(bodyCopy);

                var uri = context.Request.Path + context.Request.QueryString;

                var request = new RequestInfo
                {
                    Uri = uri,
                    Method = context.Request.Method,
                    // There is no copy-constructor.
                    Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                    ContentLength = context.Request.ContentLength,
                    ContentCopy = bodyCopy
                };

                _requestAssert(request);

                // Restore body.
                context.Request.Body = bodyBackup;

                await _next(context);

                var responseFactory = _nextResponse(uri, context.Request.Method);
                if (responseFactory is null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                }
                else
                {
                    using (var response = responseFactory(context.Request))
                    {
                        context.Response.StatusCode = response.StatusCode;

                        switch (response?.Content)
                        {
                            case string str:
                                context.Response.ContentType = "text/plain; charset=utf-8";
                                await context.Response.WriteAsync(str);
                                break;

                            case MemoryStream stream:
                                context.Response.ContentType = "application/octet-stream";
                                await stream.Rewind().CopyToAsync(context.Response.Body);
                                break;

                            default:
                                context.Response.ContentType = "application/json; charset=utf-8";
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(response?.Content));
                                break;
                        }
                    }
                }
            }
            catch (DynamicException clientEx)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync(clientEx.ToString());
            }
            catch (Exception serverEx)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync(serverEx.ToString());
            }
        }
    }
}