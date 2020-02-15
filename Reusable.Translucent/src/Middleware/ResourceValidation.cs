using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ResourceValidation : ResourceMiddleware
    {
        public ResourceValidation(RequestDelegate<ResourceContext> next, ValidateResourceDelegate validate) : base(next)
        {
            Validate = validate;
        }

        private ValidateResourceDelegate Validate { get; }

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

            await Next(context);

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

    public delegate void ValidateResourceDelegate(ResourceContext context, PipelineFlow flow);
}