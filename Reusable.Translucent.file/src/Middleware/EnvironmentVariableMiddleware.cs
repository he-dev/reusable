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
        public EnvironmentVariableMiddleware(RequestDelegate<ResourceContext> next) : base(next) { }

        public override async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request is FileRequest fileRequest)
            {
                fileRequest.ResourceName = Resolve(context.Request);
            }

            await InvokeNext(context);
        }

        private static string Resolve(Request request)
        {
            return Environment.ExpandEnvironmentVariables(request.ResourceName);
            //var normalizedPath = UriStringHelper.Normalize(expandedPath);
            //return request.ResourceName.With(x => x.Path, new UriStringComponent(normalizedPath));
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