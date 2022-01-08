using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Octopus.Abstractions;

namespace Reusable.Octopus.Nodes;

[UsedImplicitly]
public class CreateStream : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        if (context.Request.Method.In(RequestMethod.Create, RequestMethod.Update))
        {
            if (context.Request.Data.Value is CreateStreamAsync createStreamAsync)
            {
                context.Request.Data.Push(await createStreamAsync());
            }
        }

        await InvokeNext(context);
    }
}