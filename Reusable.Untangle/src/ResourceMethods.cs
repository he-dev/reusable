using System;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Translucent.Data;

namespace Reusable.Translucent;

public static class ResourceMethods
{
    public static Task<Response> ReadAsync<T>(this IResource resource, string name, object? body = default, Action<T>? configure = default) where T : Request, new()
    {
        return resource.InvokeAsync(Request.Read<T>(name, body).Also(configure));
    }

    public static Task<Response> CreateAsync<T>(this IResource resource, string name, object? body = default, Action<T>? configure = default) where T : Request, new()
    {
        return resource.InvokeAsync(Request.Create<T>(name, body).Also(configure));
    }

    public static Task<Response> UpdateAsync<T>(this IResource resource, string name, object? body = default, Action<T>? configure = default) where T : Request, new()
    {
        return resource.InvokeAsync(Request.Update<T>(name, body).Also(configure));
    }

    public static Task<Response> DeleteAsync<T>(this IResource resource, string name, object? body = default, Action<T>? configure = default) where T : Request, new()
    {
        return resource.InvokeAsync(Request.Delete<T>(name, body).Also(configure));
    }
}