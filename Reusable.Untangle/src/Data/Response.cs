using System;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Translucent.Data;

public class Response : IDisposable, IItems
{
    public string ResourceName { get; set; } = default!;

    public ResourceStatusCode StatusCode { get; set; } = ResourceStatusCode.Unknown;

    public Stack<object> Body { get; } = new();

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public static Response Success() => new() { StatusCode = ResourceStatusCode.Success };
        
    public static Response NotFound(string resourceName) => new() { ResourceName = resourceName, StatusCode = ResourceStatusCode.NotFound };

    public void Dispose()
    {
        foreach (var item in Body.Consume())
        {
            (item as IDisposable)?.Dispose();
        }
    }
}