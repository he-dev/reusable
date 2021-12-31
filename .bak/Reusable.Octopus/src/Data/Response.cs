using System;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Essentials.Data;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Data;

public class Response : IDisposable, IItems
{
    public string ResourceName { get; set; } = default!;

    public Trackable<object> Body { get; } = new();

    public ResourceStatusCode StatusCode { get; set; } = ResourceStatusCode.Unknown;

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public static Response Success(object? body = default) => new() { Body = { body }, StatusCode = ResourceStatusCode.Success };

    public static Response NotFound(string resourceName) => new() { ResourceName = resourceName, StatusCode = ResourceStatusCode.NotFound };

    public static implicit operator bool(Response response) => response.Success();
    
    public void Dispose()
    {
        Body.Dispose();
    }
}