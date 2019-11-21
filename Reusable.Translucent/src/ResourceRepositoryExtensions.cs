using System;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryExtensions
    {
        public static Task<Response> GetAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Create(RequestMethod.Get, uri, body, configureRequest));
        }

        public static Task<Response> PutAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Create(RequestMethod.Put, uri, body, configureRequest));
        }

        public static Task<Response> PostAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Create(RequestMethod.Put, uri, body, configureRequest));
        }

        public static Task<Response> DeleteAsync<T>(this IResourceRepository resources, UriString uri, object? body = default, Action<T>? configureRequest = default) where T : Request, new()
        {
            return resources.InvokeAsync(Request.Create(RequestMethod.Put, uri, body, configureRequest));
        }
    }
}