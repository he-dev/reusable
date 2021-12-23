using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Nodes;

[PublicAPI]
[UsedImplicitly]
public class ValidateRequiredResource : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        await InvokeNext(context);

        var notFound =
            context.Request.Method == RequestMethod.Read &&
            context.Request.Required() &&
            context.Response.StatusCode == ResourceStatusCode.NotFound;

        if (notFound)
        {
            throw DynamicException.Create("ResourceNotFound", $"Resource '{context.Request.ResourceName}' is required.");
        }
    }
}