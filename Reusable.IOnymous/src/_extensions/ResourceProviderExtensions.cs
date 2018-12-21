using System.Text;
using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public static class ResourceProviderExtensions
    {
        public static Task<IResourceInfo> GetFileAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            return resourceProvider.GetAsync(new UriString(PhysicalFileProvider.Scheme, path), metadata);
        }

        public static async Task<IResourceInfo> SaveFileAsync(this IResourceProvider resourceProvider, string path, string value, ResourceMetadata metadata = null)
        {
            var (stream, resourceMetadata) = ResourceHelper.CreateStream(value, metadata.GetValueOrDefault<Encoding>(nameof(Encoding)));
            using (stream)
            {
                return await resourceProvider.PutAsync(new UriString(PhysicalFileProvider.Scheme, path), stream);
            }
        }
        
        public static Task<IResourceInfo> GetAnyAsync(this IResourceProvider resourceProvider, UriString uri, ResourceMetadata metadata = null)
        {
            return resourceProvider.GetAsync(uri.With(x => x.Scheme, ResourceProvider.DefaultScheme), metadata);
        }
    }
}