using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Octopus.Abstractions;

namespace Reusable.Octopus.Nodes;

/// <summary>
/// Resolves environment variables for resource names.
/// </summary>
[UsedImplicitly]
public class ExpandEnvironmentVariables : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        if (Environment.ExpandEnvironmentVariables(context.Request.ResourceName.Value) is var name && !name.Equals(context.Request.ResourceName.Value))
        {
            context.Request.ResourceName.Push(name);
        }

        await InvokeNext(context);
    }
}