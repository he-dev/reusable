using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public delegate Task RequestCallback(IOContext context);

    // todo - works similar to composite-provider
    public class ResourceProxy
    {
        public List<IAsyncMiddleware<IOContext>> Middleware { get; set; } = new List<IAsyncMiddleware<IOContext>>();

        public IResource InvokeAsync(Request request)
        {
            //var middleware = Middleware.Aggregate((current, next) => current.InsertNext(next));
            var ioContext = new IOContext
            {
                Request = request
            };

            var middlewareTypes = new Type[0];

            var previousMiddleware = default(object);
            for (var i = middlewareTypes.Length - 1; i >= 0; i--)
            {
                var middlewareType = middlewareTypes[i];

                var next = (RequestCallback)(context => Task.CompletedTask);
                if (!(previousMiddleware is null))
                {
                    var nextInvoke = previousMiddleware.GetType().GetMethod("Invoke");
                    
                }

                previousMiddleware = Activator.CreateInstance(middlewareType, new object[] { (RequestCallback)(context => Task.CompletedTask) });
            }

            foreach (var middleware in Middleware)
            {
                var invoke = middleware.GetType().GetMethod("Invoke");
                invoke.Invoke(middleware, new[] { request });
            }

            var m = Middleware.First();

            //middleware.InvokeAsync(ioContext);

            return ioContext.Response;
        }

        private Task Invoke(IOContext context)
        {
            // todo - call next here

            return Task.CompletedTask;
        }
    }

    public class MiddlewareInvoker
    {
        public MiddlewareInvoker() { }
    }

    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(RequestMethod method) => Method = method;

        public RequestMethod Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(RequestMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(RequestMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(RequestMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(RequestMethod.Delete) { }
    }

    public interface IAsyncMiddleware<TContext> : IDisposable
    {
        bool Enabled { get; }

        IAsyncMiddleware<TContext> Previous { get; set; }

        IAsyncMiddleware<TContext> Next { get; set; }

        Task InvokeAsync(TContext context);
    }

    public static class MiddlewareHelper
    {
        public static IAsyncMiddleware<TContext> InsertNext<TContext>(this IAsyncMiddleware<TContext> current, IAsyncMiddleware<TContext> next)
        {
            (next.Previous, next.Next, current.Next) = (current, current.Next, next);
            return next;
        }

        public static IAsyncMiddleware<TContext> Remove<TContext>(this IAsyncMiddleware<TContext> current)
        {
            var result = default(IAsyncMiddleware<TContext>);

            if (!(current.Previous is null))
            {
                result = current.Previous;
                (current.Previous.Next, current.Previous) = (current.Next, null);
            }

            if (!(current.Next is null))
            {
                result = result ?? current.Next;
                (current.Next.Previous, current.Next) = (current.Previous, null);
            }

            return result;
        }
    }

    public class IOContext
    {
        public Request Request { get; set; }

        public IResource Response { get; set; }
    }

    // Support the null-pattern.
    public class TerminatorMiddleware
    {
        public bool Enabled { get; } = false;

        public IAsyncMiddleware<IOContext> Previous { get; set; }

        public IAsyncMiddleware<IOContext> Next { get; set; }

        public Task InvokeAsync(IOContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class EnvironmentVariableMiddleware
    {
        private readonly RequestCallback _next;

        public EnvironmentVariableMiddleware(RequestCallback next)
        {
            _next = next;
        }

        public bool Enabled { get; } = true;

        public async Task InvokeAsync(IOContext context)
        {
            context.Request.Uri = Resolve(context.Request.Uri);
            await _next(context);
        }

        private static UriString Resolve(UriString uri)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(uri.Path.Decoded.ToString());
            var normalizedPath = UriStringHelper.Normalize(expandedPath);
            uri = uri.With(x => x.Path, new UriStringComponent(normalizedPath));
            if (!uri.Scheme && Path.IsPathRooted(uri.Path.Decoded.ToString()))
            {
                uri = uri.With(x => x.Scheme, "file");
            }

            return uri;
        }
    }
}