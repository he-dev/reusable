using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Nodes;

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