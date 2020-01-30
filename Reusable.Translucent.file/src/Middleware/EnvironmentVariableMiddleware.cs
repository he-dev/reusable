using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Resolves environment variables for file requests.
    /// </summary>
    [UsedImplicitly]
    public class EnvironmentVariableMiddleware : MiddlewareBase
    {
        public EnvironmentVariableMiddleware(RequestDelegate<ResourceContext> next, IServiceProvider services) : base(next, services) { }

        public override async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request is FileRequest)
            {
                context.Request.Uri = Resolve(context.Request);
            }

            await InvokeNext(context);
        }

        private static UriString Resolve(Request request)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(request.Uri.Path.Decoded.ToString());
            var normalizedPath = UriStringHelper.Normalize(expandedPath);
            return request.Uri.With(x => x.Path, new UriStringComponent(normalizedPath));
        }
    }

    // public static class EnvironmentVariableMiddlewareHelper
    // {
    //     public static IPipelineBuilder<TContext> UseEnvironmentVariable<TContext>(this IPipelineBuilder<TContext> builder)
    //     {
    //         return builder.UseMiddleware<EnvironmentVariableMiddleware>();
    //     }
    // }
}