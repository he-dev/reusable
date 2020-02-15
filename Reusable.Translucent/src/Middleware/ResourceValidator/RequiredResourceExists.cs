using Reusable.Exceptionize;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware.ResourceValidator
{
    public class RequiredResourceExists : IResourceValidator
    {
        public void Validate(ResourceContext context)
        {
            var resourceNotFound =
                context.Request.Method == ResourceMethod.Read &&
                context.Request.Required() &&
                context.Response.StatusCode == ResourceStatusCode.NotFound;

            if (resourceNotFound)
            {
                throw DynamicException.Create("ResourceNotFound", $"Resource '{context.Request.ResourceName}' is required.");
            }
        }
    }
}