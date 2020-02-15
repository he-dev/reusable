using System;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public static class ResourceHelpers
    {
        public static Task<Response> GetAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreateGet<T>(uri, body).Pipe(configureRequest));
        }

        public static Task<Response> PutAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreatePut<T>(uri, body).Pipe(configureRequest));
        }

        public static Task<Response> PostAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreatePost<T>(uri, body).Pipe(configureRequest));
        }

        public static Task<Response> DeleteAsync<T>(this IResource resources, string uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreateDelete<T>(uri, body).Pipe(configureRequest));
        }
    }
}