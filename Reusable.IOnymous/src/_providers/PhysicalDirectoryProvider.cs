using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalDirectoryProvider : ResourceProvider
    {
        public PhysicalDirectoryProvider(ResourceMetadata metadata = null)
            : base(
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.CanGet, true)
                    .Add(ResourceMetadataKeys.CanPut, true)
                    .Add(ResourceMetadataKeys.CanDelete, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            ValidateScheme(uri);

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            ValidateScheme(uri);

            using (var streamReader = new StreamReader(value))
            {
                var fullName = Path.Combine(uri.Path, await streamReader.ReadToEndAsync());
                try
                {
                    Directory.CreateDirectory(fullName);
                    return await this.GetAsync(fullName, metadata);
                }
                catch (Exception inner)
                {
                    throw CreateException(this, fullName, metadata, inner);
                }
            }
        }

        public override async Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri);

            try
            {
                Directory.Delete(uri.Path, true);
                return await GetAsync(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, uri.Path, metadata, inner);
            }
        }
    }
}