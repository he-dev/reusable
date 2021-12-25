using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Nodes;

/// <summary>
/// Resolves environment variables for resource names.
/// </summary>
[UsedImplicitly]
public class ExpandEnvironmentVariables : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        if (Environment.ExpandEnvironmentVariables(context.Request.ResourceName.Peek()) is var name && !name.Equals(context.Request.CurrentName()))
        {
            context.Request.ResourceName.Push(Resolve(context.Request));
        }

        await InvokeNext(context);
    }

    private static string Resolve(Request request)
    {
        return Environment.ExpandEnvironmentVariables(request.ResourceName.Peek());
    }
}