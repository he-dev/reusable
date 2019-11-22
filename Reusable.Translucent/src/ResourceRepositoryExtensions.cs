using System;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryExtensions
    {
        public static Task<Response> GetAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreateGet<T>(uri, body).Pipe(configureRequest));
        }

        public static Task<Response> PutAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreatePut<T>(uri, body).Pipe(configureRequest));
        }

        public static Task<Response> PostAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreatePost<T>(uri, body).Pipe(configureRequest));
        }

        public static Task<Response> DeleteAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.CreateDelete<T>(uri, body).Pipe(configureRequest));
        }
    }
}