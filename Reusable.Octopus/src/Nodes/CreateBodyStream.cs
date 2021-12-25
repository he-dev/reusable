using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Nodes;

[UsedImplicitly]
public class CreateBodyStream : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        if (context.Request.Method == RequestMethod.Create && context.Request.CurrentBody() is BodyStreamFunc createBodyStream)
        {
            context.Request.Body.Push(await createBodyStream());
        }

        await InvokeNext(context);
    }
}