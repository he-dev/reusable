using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryExtensions
    {
        public static Task<Response> CreateAsync(this IResourceRepository resourceRepository, UriString uri, object body, IImmutableContainer? context = default)
        {
            return resourceRepository.InvokeAsync(new Request.Get(uri)
            {
                Body = body,
                Metadata = context.ThisOrEmpty(),
            });
        }

//        public static Task<Response> ReadAsync(this IResourceRepository resourceRepository, Request request)
//        {
//            return default;
//        }
//
//        public static Task<Response> UpdateAsync(this IResourceRepository resourceRepository, Request request)
//        {
//            return default;
//        }
//
//        public static Task<Response> DeleteAsync(this IResourceRepository resourceRepository, Request request)
//        {
//            return default;
//        }
    }
}