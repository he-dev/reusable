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
    internal IResourceNode First { get; init; } = null!;

    public async Task<Response> InvokeAsync(Request request)
    {
        if (request.ResourceName.Any() == false) throw new ArgumentOutOfRangeException(nameof(request), $"Resource name must not be null nor empty.");
        if (request.Method == RequestMethod.None) throw new ArgumentOutOfRangeException(nameof(request), $"Request method must not be '{nameof(RequestMethod.None)}'.");

        var context = new ResourceContext { Request = request };

        context.Request.Log("Start.");
        await First.InvokeAsync(context).ConfigureAwait(false);
        
        return context.Response;
    }

    public class Builder : List<IResourceNode>
    {
        public IResource Build() => new Resource { First = this.Join().First() };
    }
}