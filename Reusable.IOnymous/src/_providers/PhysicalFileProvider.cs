using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : ResourceProvider
    {
        public static readonly string Scheme = "file";

        public PhysicalFileProvider(ResourceMetadata metadata = null)
            : base(new SoftString[] { "file" }, (metadata ?? ResourceMetadata.Empty))
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

    [PublicAPI]
    internal class PhysicalFileInfo : ResourceInfo
    {
        // todo - add support for other file types
        public PhysicalFileInfo([NotNull] UriString uri)
            : base(uri, ResourceFormat.String)
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
    }
}