using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class SettingModelValidationMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public SettingModelValidationMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            await _next(context);

            if (context.Request.Uri.Scheme.Equals("config")) { }
        }
    }
}