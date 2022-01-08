using System;
using Reusable.Essentials;
using Reusable.Octopus.Data;

namespace Reusable.Octopus;

public static class RequestConfiguration
{
    public static Action<T> Body<T>(this Action<T> configure, object body) where T : Request => configure.Then(request => request.Data.Push(body));

    public static Action<T> Serializer<T>(this Action<T> configure, string name) where T : Request => configure.Then(request => request.Items[nameof(Serializer)] = name);

    public static Action<T> As<T>(this Action<T> configure, Type type) where T : Request => configure.Then(request => request.Items[nameof(As)] = type);
}