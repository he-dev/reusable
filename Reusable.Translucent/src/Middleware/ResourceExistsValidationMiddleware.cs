using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ResourceExistsValidationMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public ResourceExistsValidationMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            await _next(context);

            if (context.Response.Exists())
            {
                return;
            }
            
            if (context.Request.Metadata.GetItemOrDefault(Request.IsOptional))
            {
                return;
            }

            throw DynamicException.Create("ResourceNotFound", $"Could not find resource '{context.Request.Uri}'.");
        }
    }
}