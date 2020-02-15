using System;
using System.ComponentModel.DataAnnotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Middleware.ResourceValidators
{
    public class SettingAttributeValidator : IResourceValidator
    {
        public void Validate(ResourceContext context)
        {
            if (context.Request is ConfigRequest config)
            {
                var body = context.Processed switch
                {
                    false => context.Request.Body,
                    true => context.Response.Body
                };

                foreach (var validation in config.ValidationAttributes)
                {
                    try
                    {
                        validation.Validate(body, new ValidationContext(body));
                    }
                    catch (Exception inner)
                    {
                        throw DynamicException.Create
                        (
                            "SettingValidation",
                            $"Setting '{config.ResourceName}' does not pass the '{validation.GetType().ToPrettyString()}' validation. See the inner exception for details.",
                            inner
                        );
                    }
                }
            }
        }
    }
}