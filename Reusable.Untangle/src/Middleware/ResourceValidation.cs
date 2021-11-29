using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ResourceValidation : ResourceMiddleware
    {
        public ResourceValidation(IResourceValidator validator) => Validator = validator;

        private IResourceValidator Validator { get; }

        public override async Task InvokeAsync(ResourceContext context)
        {
            try
            {
                Validator.Validate(context);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("RequestValidation", $"Resource '{context.Request.ResourceName}' is invalid. See the inner exception for details.", inner);
            }

            await InvokeNext(context);

            try
            {
                Validator.Validate(context);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("ResponseValidation", $"Resource '{context.Request.ResourceName}' is invalid. See the inner exception for details.", inner);
            }
        }
    }

    public static class ResourceValidationHelper
    {
        public static void Required(this Request request, bool required)
        {
            request.Items[nameof(Required)] = required;
        }

        public static bool Required(this Request request)
        {
            return request.Items.TryGetValue(nameof(Required), out var value) && value is bool required && required;
        }
    }
}