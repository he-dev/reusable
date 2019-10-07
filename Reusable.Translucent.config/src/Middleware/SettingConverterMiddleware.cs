using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OneTo1;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class SettingConverterMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public SettingConverterMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            await _next(context);

            if (!context.Request.Method.Equals(RequestMethod.Get))
            {
                return;
            }

            if (!context.Request.Uri.Scheme.Equals(ConfigController.Scheme))
            {
                return;
            }

            using (var response = context.Response)
            {
                var bodyConverter = response.Metadata.GetItem(Resource.Converter);
                var bodyType = response.Metadata.GetItem(Resource.Type);

                // Replace raw response with deserialized one.
                context.Response = new Response
                {
                    StatusCode = response.StatusCode,
                    Metadata = response.Metadata,
                    Body = bodyConverter.Convert(response.Body, bodyType)
                };
            }
        }
    }
}