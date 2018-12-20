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

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            using (var fileStream = new FileStream(uri.Path.Decoded, FileMode.CreateNew, FileAccess.Write))
            {
                await value.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(uri, metadata);
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            File.Delete(uri.Path.Decoded);
            return await GetAsync(uri, metadata);
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

        public override bool Exists => File.Exists(Uri.Path.Decoded);

        public override long? Length => new FileInfo(Uri.Path.Decoded).Length;

        public override DateTime? CreatedOn => Exists ? File.GetCreationTimeUtc(Uri.Path.Decoded) : default;

        public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc(Uri.Path.Decoded) : default;

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            using (var fileStream = File.OpenRead(Uri.Path.Decoded))
            {
                await fileStream.CopyToAsync(stream);
            }
        }

        protected override async Task<object> DeserializeAsyncInternal(Type targetType)
        {
            using (var fileStream = File.OpenRead(Uri.Path.Decoded))
            using (var streamReader = new StreamReader(fileStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}