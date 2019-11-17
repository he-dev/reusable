using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class EnvironmentVariableMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public EnvironmentVariableMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request.Uri.Scheme.Equals("file"))
            {
                context.Request.Uri = Resolve(context.Request.Uri);
            }

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

    public static class EnvironmentVariableMiddlewareHelper
    {
        public static IPipelineBuilder<TContext> UseEnvironmentVariables<TContext>(this IPipelineBuilder<TContext> builder)
        {
            return builder.UseMiddleware<EnvironmentVariableMiddleware>();
        }
    }
}