using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus;
using Reusable.Octopus.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Nodes;

public class ValidateSetting : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        if (context.Request is ConfigRequest config)
        {
            var required = config.ValidationAttributes.OfType<RequiredAttribute>().FirstOrDefault();

            if (context.Request.Method == RequestMethod.Update)
            {
                if (context.Request.Body.TryPeek(out var newValue))
                {
                    Validate(config.ResourceName.Peek(), newValue, config.ValidationAttributes);
                }
                else
                {
                    throw DynamicException.Create("SettingValidation", $"Setting '{config.ResourceName.Peek()}' is required.");
                }
            }

            await InvokeNext(context);

            if (context.Request.Method == RequestMethod.Read)
            {
                if (required is { } && context.Response.StatusCode == ResourceStatusCode.NotFound)
                {
                    throw DynamicException.Create("SettingValidation", $"Setting '{config.ResourceName.Peek()}' is required.");
                }
                else
                {
                    if (context.Response.Body.TryPeek(out var value))
                    {
                        Validate(config.ResourceName.Peek(), value, config.ValidationAttributes);
                    }
                }
            }
        }
        else
        {
            await InvokeNext(context);
        }
    }

    private static void Validate(string resourceName, object value, IEnumerable<ValidationAttribute> validationAttributes)
    {
        foreach (var validation in validationAttributes)
        {
            if (!validation.IsValid(value))
            {
                throw DynamicException.Create("SettingValidation", $"Setting '{resourceName}' didn't pass the '{validation.GetType().ToPrettyString()}'.");
            }
        }
    }
}