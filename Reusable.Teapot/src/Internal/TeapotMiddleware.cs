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

namespace Reusable.Teapot.Internal
{
    internal class RequestLog : ConcurrentDictionary<PathString, IImmutableList<RequestInfo>> { }

    internal delegate IEnumerable<IObserver<RequestInfo>> ObserversDelegate();

    internal class TeapotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ObserversDelegate _getObservers;

        public TeapotMiddleware(RequestDelegate next, ObserversDelegate getObservers)
        {
            _next = next;
            _getObservers = getObservers;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // We'll need this later so don't dispose.
                var memory = new MemoryStream();
                {
                    // It needs to be copied because otherwise it'll get disposed.
                    await context.Request.Body.CopyToAsync(memory);

                    // Each observer gets it's own request copy.
                    foreach (var observer in _getObservers())
                    {
                        var request = new RequestInfo
                        {
                            Path = context.Request.Path,
                            ContentLength = context.Request.ContentLength,
                            // There is no copy-constructor.
                            Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                            BodyStreamCopy = memory
                        };

                        observer.OnNext(request);
                    }

                    await _next(context);
                    context.Response.StatusCode = StatusCodes.Status418ImATeapot;
                }

            }
            catch (Exception inner)
            {
                foreach (var observer in _getObservers())
                {
                    observer.OnError(inner);
                }
                // Not sure what to do... throw or not?
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                //throw;
            }
        }

    }
}