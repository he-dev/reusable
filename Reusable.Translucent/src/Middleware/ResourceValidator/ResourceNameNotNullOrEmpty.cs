using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Middleware.ResourceValidator
{
    public class ResourceNameNotNullOrEmpty : IResourceValidator
    {
        public void Validate(ResourceContext context)
        {
            if (!context.Processed && string.IsNullOrEmpty(context.Request.ResourceName))
            {
                throw DynamicException.Create
                (
                    "ResourceNameMissing",
                    $"{context.Request.GetType().ToPrettyString()} for does not specify the resource-name."
                );
            }
        }
    }
}