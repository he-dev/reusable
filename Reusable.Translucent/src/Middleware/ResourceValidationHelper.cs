using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    public static class ResourceValidationHelper
    {
        public static void ValidateResourceExists(ResourceContext context, PipelineFlow flow)
        {
            var resourceNotFound =
                context.Request.Method == ResourceMethod.Get &&
                context.Request.GetItemOrDefault("Required", false) &&
                context.Response.StatusCode == ResourceStatusCode.NotFound;

            if (resourceNotFound)
            {
                throw DynamicException.Create("ResourceNotFound", $"Resource '{context.Request.ResourceName}' is required.");
            }
        }

        public static void ValidateRequestContainsMethod(ResourceContext context, PipelineFlow flow)
        {
            switch (flow)
            {
                case PipelineFlow.Request:
                    if (context.Request.Method is null || context.Request.Method == Option<ResourceMethod>.None)
                    {
                        throw DynamicException.Create
                        (
                            "ResourceMethodMissing",
                            $"{context.Request.GetType().ToPrettyString()} for '{context.Request.ResourceName}' does not specify the method."
                        );
                    }
                    break;
            }
        }
        
        public static void ValidateRequestContainsName(ResourceContext context, PipelineFlow flow)
        {
            switch (flow)
            {
                case PipelineFlow.Request:
                    if (string.IsNullOrEmpty(context.Request.ResourceName))
                    {
                        throw DynamicException.Create
                        (
                            "ResourceNameMissing",
                            $"{context.Request.GetType().ToPrettyString()} for does not specify the resource-name."
                        );
                    }
                    break;
            }
        }

        public static ValidateResourceDelegate Composite(params ValidateResourceDelegate[] validations)
        {
            return (context, flow) =>
            {
                foreach (var validation in validations)
                {
                    validation(context, flow);
                }
            };
        }
    }
}