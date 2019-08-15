using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent
{
    // Provides CRUD APIs.
    public static partial class ResourceSquidExtensions
    {
        public static Task<Response> CreateAsync(this IResourceSquid resourceSquid, UriString uri, object body, CreateStreamCallback createBodyStreamCallback, IImmutableContainer context = default)
        {
            return resourceSquid.InvokeAsync(new Request.Get(uri)
            {
                Body = body,
                Metadata = context.ThisOrEmpty(),
                CreateBodyStreamCallback = createBodyStreamCallback
            });
        }

        public static Task<Response> ReadAsync(this IResourceSquid resourceSquid, Request request)
        {
            return default;
        }

        public static Task<Response> UpdateAsync(this IResourceSquid resourceSquid, Request request)
        {
            return default;
        }

        public static Task<Response> DeleteAsync(this IResourceSquid resourceSquid, Request request)
        {
            return default;
        }
    }
}