using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Resolves environment variables for file requests.
    /// </summary>
    [UsedImplicitly]
    public class EnvironmentVariableMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public EnvironmentVariableMiddleware(RequestDelegate<ResourceContext> next) => _next = next;

        public async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request.Uri.Scheme.Equals("file"))
            {
                context.Request.Uri = Resolve(context.Request);
            }

            await _next(context);
        }

        private static UriString Resolve(Request request)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(request.Uri.Path.Decoded.ToString());
            var normalizedPath = UriStringHelper.Normalize(expandedPath);
            return request.Uri.With(x => x.Path, new UriStringComponent(normalizedPath));
        }
    }

    public static class EnvironmentVariableMiddlewareHelper
    {
        public static IPipelineBuilder<TContext> UseEnvironmentVariable<TContext>(this IPipelineBuilder<TContext> builder)
        {
            return builder.UseMiddleware<EnvironmentVariableMiddleware>();
        }
    }
}