using System;
using System.Collections.Generic;
using System.Threading;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Octopus.Data;

using static RequestMethod;

public interface IItems
{
    IDictionary<string, object> Items { get; }
}

public abstract class Request : IDisposable, IItems
{
    public RequestMethod Method { get; protected init; } = None;

    public Stack<string> ResourceName { get; } = new();

    /// <summary>
    /// Gets or sets the object that should be handled. Can be anything or a Stream.
    /// </summary>
    public Stack<object> Body { get; } = new();

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public IControllerFilter? ControllerFilter { get; set; }

    public bool AllowControllerCaching { get; set; } = true;

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public static T Read<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Read, ResourceName = { name }, Body = { body } };
    public static T Create<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Create, ResourceName = { name }, Body = { body } };
    public static T Update<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Update, ResourceName = { name }, Body = { body } };
    public static T Delete<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Delete, ResourceName = { name }, Body = { body } };

    public void Dispose()
    {
        foreach (var item in Body.Consume())
        {
            (item as IDisposable)?.Dispose();
        }
    }
}

public interface IJsonRequest
{
    Type BodyType { get; }
}