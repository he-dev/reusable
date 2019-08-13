using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    // Provides CRUD APIs.
    public static class ResourceRepositoryExtensions
    {
        public static Task<IResource> CreateAsync(this ResourceRepository resourceRepository, UriString uri, object body, CreateStreamCallback createBodyStreamCallback, IImmutableContainer context = default)
        {
            return resourceRepository.InvokeAsync(new Request.Get(uri)
            {
                Body = body,
                Context = context.ThisOrEmpty(),
                CreateBodyStreamCallback = createBodyStreamCallback
            });
        }

        public static Task<IResource> ReadAsync(this ResourceRepository resourceRepository, Request request)
        {
            return default;
        }

        public static Task<IResource> UpdateAsync(this ResourceRepository resourceRepository, Request request)
        {
            return default;
        }

        public static Task<IResource> DeleteAsync(this ResourceRepository resourceRepository, Request request)
        {
            return default;
        }
    }
}