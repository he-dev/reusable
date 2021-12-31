using System.IO;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Nodes;

using static RequestMethod;

[UsedImplicitly]
public class ManageString : ResourceNode
{
    public override async Task InvokeAsync(ResourceContext context)
    {
        if (context.Request.Method.In(Create, Update))
        {
            if (context.Request.Data.TryPeek(out var value) && value is string text)
            {
                context.Request.Data.Push(text.ToMemoryStream());
            }
        }

        await InvokeNext(context);

        if (context.Request.Method.In(Read) && context.Response)
        {
            if (context.Response.Body.TryPeek(out var value) && value is Stream stream && context.Request.ItemEquals("As", typeof(string)))
            {
                context.Response.Body.Push(await stream.ReadTextAsync());
            }
        }
    }
}