using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.Teapot.Internal
{
    public class RequestLog : ConcurrentDictionary<(PathString, SoftString), IImmutableList<RequestInfo>> { }

    public class ResponseQueue : ConcurrentDictionary<(PathString, SoftString), Queue<Func<ResponseInfo>>> { }


    //internal delegate IEnumerable<IObserver<RequestInfo>> ObserversDelegate();

    public delegate void LogDelegate(PathString path, SoftString method, RequestInfo request);

    public delegate Func<ResponseInfo> ResponseDelegate(PathString path, SoftString method);


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
                // We'll need this later restore body so don't dispose.
                var bodyBackup = new MemoryStream();

                // It needs to be copied because the original stream does not support seeking.
                await context.Request.Body.CopyToAsync(bodyBackup);

                // Each log gets it's own request copy. It's easier to dispose them later.
                bodyBackup.Seek(0, SeekOrigin.Begin);
                var bodyCopy = new MemoryStream();
                await bodyBackup.CopyToAsync(bodyCopy);

                var request = new RequestInfo
                {
                    //Path = context.Request.Path,
                    ContentLength = context.Request.ContentLength,
                    // There is no copy-constructor.
                    Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                    BodyStreamCopy = bodyCopy
                };

                _log(context.Request.Path, context.Request.Method, request);

                // Restore body.
                context.Request.Body = bodyBackup;

                await _next(context);

                var response = _nextResponse(context.Request.Path, context.Request.Method)();

                // todo - configure response here
                context.Response.StatusCode = response.IsEmpty ? StatusCodes.Status404NotFound : response.StatusCode;

                switch (response.Content)
                {
                    case string str:
                        await context.Response.WriteAsync(str);
                        break;

                    case object obj:
                        context.Response.ContentType = "application/json; charset=utf-8";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(obj));
                        break;
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