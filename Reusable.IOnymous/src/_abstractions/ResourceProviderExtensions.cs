using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public static class ResourceProviderExtensions
    {
        public static IResourceProvider DecorateWith(this IResourceProvider decorable, Func<IResourceProvider, IResourceProvider> createDecorator)
        {
            return createDecorator(decorable);
        }
        
        public static Task<IResourceInfo> GetFileAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var uri = Path.IsPathRooted(path) ? new UriString(PhysicalFileProvider.Scheme, path) : new UriString(path);
            return resourceProvider.GetAsync(uri, metadata);
        }

        public static async Task<IResourceInfo> SaveFileAsync(this IResourceProvider resourceProvider, string path, string value, ResourceMetadata metadata = null)
        {
            var (stream, resourceMetadata) = ResourceHelper.CreateStream(value, metadata.GetValueOrDefault<Encoding>(nameof(Encoding)));
            using (stream)
            {
                var uri = Path.IsPathRooted(path) ? new UriString(PhysicalFileProvider.Scheme, path) : new UriString(path);
                return await resourceProvider.PutAsync(uri, stream);
            }
        }

        public static Task<IResourceInfo> GetAnyAsync(this IResourceProvider resourceProvider, UriString uri, ResourceMetadata metadata = null)
        {
            return resourceProvider.GetAsync(uri.With(x => x.Scheme, ResourceProvider.DefaultScheme), metadata);
        }

        public static T GetFile<T>(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var file = resourceProvider.GetFileAsync(path, metadata).GetAwaiter().GetResult();
            return file.DeserializeAsync<T>().GetAwaiter().GetResult();
        }
        
        
        public static Task<IResourceInfo> GetHttpAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var uri = new UriString("http", path);
            return resourceProvider.GetAsync(uri, metadata);
        }
    }
}