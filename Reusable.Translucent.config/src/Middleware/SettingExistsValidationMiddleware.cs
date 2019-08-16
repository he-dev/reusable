using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class SettingExistsValidationMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public SettingExistsValidationMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            await _next(context);

            if (context.Request.Uri.Scheme.Equals("config"))
            {
                if (!context.Request.Metadata.GetItemOrDefault(ConfigRequest.IsOptional) && !context.Response.Exists())
                {
                    throw DynamicException.Create("SettingNotFound", $"Could not find setting '{context.Request.Uri}'.");
                }
            }
        }
    }
}