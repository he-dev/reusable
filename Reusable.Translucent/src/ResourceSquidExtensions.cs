using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidExtensions
    {
        public static Task<Response> CreateAsync(this IResourceSquid resourceSquid, UriString uri, object body, IImmutableContainer context = default)
        {
            return resourceSquid.InvokeAsync(new Request.Get(uri)
            {
                Body = body,
                Metadata = context.ThisOrEmpty(),
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