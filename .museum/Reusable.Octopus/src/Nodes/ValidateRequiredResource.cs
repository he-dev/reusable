using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Nodes;

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