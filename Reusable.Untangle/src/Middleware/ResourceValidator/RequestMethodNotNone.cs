using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Middleware.ResourceValidator
{
    public class RequestMethodNotNone : IResourceValidator
    {
        public void Validate(ResourceContext context)
        {
            if (!context.Processed && context.Request.Method == ResourceMethod.None)
            {
                throw DynamicException.Create
                (
                    "ResourceMethodMissing",
                    $"{context.Request.GetType().ToPrettyString()} for '{context.Request.ResourceName}' does not specify the method."
                );
            }
        }
    }
}