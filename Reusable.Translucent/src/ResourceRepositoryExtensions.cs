using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryExtensions
    {
        public static Task<Response> GetAsync(this IResourceRepository resources, UriString uri, object? body = default, IImmutableContainer? metadata = default)
        {
            return resources.InvokeAsync(new Request.Get(uri) { Body = body, Metadata = metadata.ThisOrEmpty() });
        }

        public static Task<Response> PutAsync(this IResourceRepository resources, UriString uri, object? body = default, IImmutableContainer? metadata = default)
        {
            return resources.InvokeAsync(new Request.Put(uri) { Body = body, Metadata = metadata.ThisOrEmpty() });
        }

        public static Task<Response> PostAsync(this IResourceRepository resources, UriString uri, object? body = default, IImmutableContainer? metadata = default)
        {
            return resources.InvokeAsync(new Request.Post(uri) { Body = body, Metadata = metadata.ThisOrEmpty() });
        }

        public static Task<Response> DeleteAsync(this IResourceRepository resources, UriString uri, object? body = default, IImmutableContainer? metadata = default)
        {
            return resources.InvokeAsync(new Request.Delete(uri) { Body = body, Metadata = metadata.ThisOrEmpty() });
        }
    }
}