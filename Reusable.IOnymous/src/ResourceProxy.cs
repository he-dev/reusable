using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    // todo - works similar to composite-provider
    public class ResourceProxy
    {
        public List<IAsyncMiddleware<IOContext>> Middleware { get; set; } = new List<IAsyncMiddleware<IOContext>>();

        public IResource InvokeAsync(Request request)
        {
            var middleware = Middleware.Aggregate((current, next) => current.InsertNext(next));
            var ioContext = new IOContext
            {
                Request = request
            };
            middleware.InvokeAsync(ioContext);

            return ioContext.Response;
        }
    }

    public class ResourceActionAttribute : Attribute { }

    public class ResourceGetAttribute : ResourceActionAttribute { }

    public class ResourcePostAttribute : ResourceActionAttribute { }

    public class ResourcePutAttribute : ResourceActionAttribute { }

    public class ResourceDeleteAttribute : ResourceActionAttribute { }

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

    public class EnvironmentVariableMiddleware : IAsyncMiddleware<IOContext>
    {
        public EnvironmentVariableMiddleware() { }

        public bool Enabled { get; } = true;

        public IAsyncMiddleware<IOContext> Previous { get; set; }

        public IAsyncMiddleware<IOContext> Next { get; set; }

        public Task InvokeAsync(IOContext context)
        {
            context.Request.Uri = Resolve(context.Request.Uri);
            Next?.InvokeAsync(context);
            return Task.CompletedTask;
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

        public void Dispose() => this.Remove();
    }
}