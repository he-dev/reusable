using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    // Provides CRUD APIs.
    public static partial class ResourceSquidExtensions
    {
        public static Task<IResource> CreateAsync(this IResourceSquid resourceSquid, UriString uri, object body, CreateStreamCallback createBodyStreamCallback, IImmutableContainer context = default)
        {
            return resourceSquid.InvokeAsync(new Request.Get(uri)
            {
                Body = body,
                Metadata = context.ThisOrEmpty(),
                CreateBodyStreamCallback = createBodyStreamCallback
            });
        }

        public static Task<IResource> ReadAsync(this IResourceSquid resourceSquid, Request request)
        {
            return default;
        }

        public static Task<IResource> UpdateAsync(this IResourceSquid resourceSquid, Request request)
        {
            return default;
        }

        public static Task<IResource> DeleteAsync(this IResourceSquid resourceSquid, Request request)
        {
            return default;
        }
    }
}