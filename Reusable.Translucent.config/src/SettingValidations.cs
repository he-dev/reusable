using System;
using System.ComponentModel.DataAnnotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class SettingValidations
    {
        public static void ValidateByAttributes(ResourceContext context, PipelineFlow flow)
        {
            if (context.Request is ConfigRequest config)
            {
                var body = flow switch
                {
                    PipelineFlow.Request => context.Request.Body,
                    PipelineFlow.Response => context.Response.Body
                };

                foreach (var validation in config.ValidationAttributes)
                {
                    try
                    {
                        validation.Validate(body, new ValidationContext(body));
                    }
                    catch (Exception inner)
                    {
                        throw DynamicException.Create("SettingValidation", $"Setting '{config.ResourceName}' does not pass the '{validation.GetType().ToPrettyString()}' validation. See the inner exception for details.", inner);
                    }
                }
            }
        }
    }
}