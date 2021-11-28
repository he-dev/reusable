using System;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public static class ResourceHelper
    {
        public static Task<Response> CreateAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configure = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Create<T>(uri, body).Also(configure));
        }

        public static Task<Response> ReadAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configure = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Read<T>(uri, body).Also(configure));
        }

        public static Task<Response> UpdateAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configure = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Update<T>(uri, body).Also(configure));
        }

        public static Task<Response> DeleteAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configure = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Delete<T>(uri, body).Also(configure));
        }
    }
}