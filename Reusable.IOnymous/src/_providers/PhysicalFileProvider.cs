using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : ResourceProvider
    {
        public new static readonly string Scheme = "file";

        public PhysicalFileProvider(ResourceMetadata metadata = null)
            : base(
                (metadata ?? ResourceMetadata.Empty)
                .Add(ResourceMetadataKeys.CanGet, true)
                .Add(ResourceMetadataKeys.CanPut, true)
                .Add(ResourceMetadataKeys.CanDelete, true)
                .Add(ResourceMetadataKeys.Scheme, Scheme)
            )
        {
        }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, Scheme);

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, Scheme);

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
            ValidateScheme(uri, Scheme);

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

    public static class ResourceProviderExtensions
    {
        public static Task<IResourceInfo> GetFileInfoAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            return resourceProvider.GetAsync((PhysicalFileProvider.Scheme, path), metadata);
        }

        public static async Task<IResourceInfo> SaveFileAsync(this IResourceProvider resourceProvider, string path, string value, ResourceMetadata metadata = null)
        {
            var (stream, resourceMetadata) = ResourceHelper.CreateStream(value, metadata.GetValueOrDefault<Encoding>(nameof(Encoding)));
            using (stream)
            {
                return await resourceProvider.PutAsync((PhysicalFileProvider.Scheme, path), stream);
            }
        }
    }

    [PublicAPI]
    internal class PhysicalFileInfo : ResourceInfo
    {
        public PhysicalFileInfo([NotNull] UriString uri)
            : base(uri)
        {
        }

        public override bool Exists => File.Exists(Uri.Path);

        public override long? Length => new FileInfo(Uri.Path).Length;

        public override DateTime? CreatedOn => Exists ? File.GetCreationTimeUtc(Uri.Path) : default;

        public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc(Uri.Path) : default;

        public override async Task CopyToAsync(Stream stream)
        {
            AssertExists();

            using (var fileStream = File.OpenRead(Uri.Path))
            {
                await fileStream.CopyToAsync(stream);
            }
        }

        public override async Task<object> DeserializeAsync(Type targetType)
        {
            AssertExists();

            using (var fileStream = File.OpenRead(Uri.Path))
            using (var streamReader = new StreamReader(fileStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}