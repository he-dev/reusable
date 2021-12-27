using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus;

[PublicAPI]
public interface IResource
{
    Task<Response> InvokeAsync(Request request);
}

[PublicAPI]
public class Resource : IResource
{
    private IResourceNode First { get; init; } = null!;

    public async Task<Response> InvokeAsync(Request request)
    {
        var context = new ResourceContext(request).Also(x => x.Request.Log("Start."));

        await First.InvokeAsync(context).ConfigureAwait(false);

        return context.Response;
    }

    public class Builder : List<IResourceNode>
    {
        public IResource Build() => new Resource { First = this.Join().First() };
    }
}