using System;
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
        await InvokeNext(context);

        if (context.Request is ConfigRequest config)
        {
            foreach (var validation in config.ValidationAttributes)
            {
                try
                {
                    //validation.Validate(body, new ValidationContext(body));
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create
                    (
                        "SettingValidation",
                        $"Setting '{config.ResourceName}' didn't pass the '{validation.GetType().ToPrettyString()}' validation. See the inner exception for details.",
                        inner
                    );
                }
            }
        }
    }
}