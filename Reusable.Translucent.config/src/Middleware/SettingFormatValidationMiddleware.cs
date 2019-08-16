using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class SettingFormatValidationMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public SettingFormatValidationMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            await _next(context);

            if (context.Request.Uri.Scheme.Equals("config"))
            {
                if (!(context.Response.Body is null) && !(context.Response.Body is string _))
                {
                    throw DynamicException.Create("SettingFormat", $"Setting '{context.Request.Uri}' has an invalid format '{context.Response.Body?.GetType().ToPrettyString()}'. Must be a 'string'.");
                }
            }
        }
    }
}