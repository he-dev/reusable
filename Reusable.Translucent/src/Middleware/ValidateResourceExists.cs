using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Validates that a required resource exists and throws if it is not the case. Handles only GET requests.
    /// </summary>
    [UsedImplicitly]
    public class ValidateResourceExists : MiddlewareBase
    {
        public ValidateResourceExists(RequestDelegate<ResourceContext> next, IServiceProvider services) : base(next, services) { }

        public override async Task InvokeAsync(ResourceContext context)
        {
            await InvokeNext(context);

            if (context.Request.Method == RequestMethod.Get && context.Request.Required && !context.Response.Exists())
            {
                throw DynamicException.Create("ResourceNotFound", $"Could not find resource '{context.Request.Uri}'.");
            }
        }
    }
}