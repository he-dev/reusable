using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Exceptionize;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ResourceValidation : MiddlewareBase
    {
        public ResourceValidation(RequestDelegate<ResourceContext> next, ValidateResourceDelegate validate) : base(next)
        {
            Validate = validate;
        }

        public ValidateResourceDelegate Validate { get; }
        
        public override async Task InvokeAsync(ResourceContext context)
        {
            try
            {
                Validate(context, PipelineFlow.Request);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("RequestValidation", $"Resource '{context.Request.ResourceName}' is invalid. See the inner exception for details.", inner);
            }
            
            await InvokeNext(context);

            try
            {
                Validate(context, PipelineFlow.Response);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("ResponseValidation", $"Resource '{context.Request.ResourceName}' is invalid. See the inner exception for details.", inner);
            }
        }
    }

    public enum PipelineFlow
    {
        Request,
        Response
    }

    public static class ResourceValidations
    {
        public static void Exists(ResourceContext context, PipelineFlow flow)
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

    public delegate void ValidateResourceDelegate(ResourceContext context, PipelineFlow flow);
}