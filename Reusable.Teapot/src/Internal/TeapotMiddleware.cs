using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.Teapot.Internal
{
    public class RequestLog : ConcurrentDictionary<(PathString, SoftString), IImmutableList<RequestInfo>>
    {
    }

    public class ResponseQueue : ConcurrentDictionary<(string, SoftString), Queue<Func<HttpRequest, ResponseInfo>>>
    {
    }


    //internal delegate IEnumerable<IObserver<RequestInfo>> ObserversDelegate();

    public delegate void LogDelegate(string path, SoftString method, RequestInfo request);

    public delegate Func<HttpRequest, ResponseInfo> ResponseDelegate(string path, SoftString method);

    [UsedImplicitly]
    internal class TeapotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LogDelegate _log;
        private readonly ResponseDelegate _nextResponse;

        public TeapotMiddleware(RequestDelegate next, LogDelegate log, ResponseDelegate nextResponse)
        {
            _next = next;
            _log = log;
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
                bodyBackup.Seek(0, SeekOrigin.Begin);
                var bodyCopy = new MemoryStream();
                await bodyBackup.CopyToAsync(bodyCopy);

                var fullUri = context.Request.Path + context.Request.QueryString;

                var request = new RequestInfo
                {
                    ContentLength = context.Request.ContentLength,
                    // There is no copy-constructor.
                    Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                    ContentCopy = bodyCopy
                };

                _log(fullUri, context.Request.Method, request);

                // Restore body.
                context.Request.Body = bodyBackup;

                await _next(context);

                using (var response = _nextResponse(fullUri, context.Request.Method)(context.Request))
                {
                    context.Response.StatusCode =
                        response.IsEmpty
                            ? StatusCodes.Status404NotFound
                            : response.StatusCode;

                    switch (response.Content)
                    {
                        case string str:
                            //context.Response.ContentType = "application/json; charset=utf-8";
                            await context.Response.WriteAsync(str);
                            break;

                        case MemoryStream stream:
                            context.Response.ContentType = "application/json; charset=utf-8";
                            stream.Seek(0, SeekOrigin.Begin);
                            await stream.CopyToAsync(context.Response.Body);
                            break;

                        default:
                            context.Response.ContentType = "application/json; charset=utf-8";
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(response.Content));
                            break;
                    }
                }
            }
            catch (Exception inner)
            {
                // Not sure what to do... throw or not?
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                //throw;
            }
        }
    }
}