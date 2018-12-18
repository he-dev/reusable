using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : ResourceProvider
    {
        public PhysicalFileProvider(ResourceMetadata metadata = null)
            : base(
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.CanGet, true)
                    .Add(ResourceMetadataKeys.CanPut, true)
                    .Add(ResourceMetadataKeys.CanDelete, true)
                    .Add(Scheme, "file")
            )
        { }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, "file");

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, "file");

            try
            {
                using (var fileStream = new FileStream(uri.Path, FileMode.CreateNew, FileAccess.Write))
                {
                    await value.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
                return await GetAsync(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, uri.Path, metadata, inner);
            }

        }

        public override async Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, "file");
            
            try
            {
                File.Delete(uri.Path);
                return await GetAsync(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, uri.Path, metadata, inner);
            }
        }
    }

    public static class ResouceProviderExtensions
    {
        public static Task<IResourceInfo> GetFileInfoAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            return resourceProvider.GetAsync($"file:{path.Replace('\\', '/')}", metadata);
        }
    }
}
