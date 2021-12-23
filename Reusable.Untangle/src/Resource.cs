using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Nodes;

namespace Reusable.Translucent;

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

        var log = new List<string> { "Start." };

        var context = new ResourceContext { Request = request }.Also(c =>
        {
            // Use the same instance for both so we don't have to merge it later.
            c.Request.Items["Log"] = log;
            c.Response.Items["Log"] = log;
        });

        await First.InvokeAsync(context);
        return context.Response;
    }

    public class Builder : List<IResourceNode>
    {
        public IResource Build() => new Resource { First = this.Join().First() };
    }
}