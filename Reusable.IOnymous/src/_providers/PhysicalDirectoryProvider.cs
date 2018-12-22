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
            : base(new SoftString[] { "directory" }, (metadata ?? ResourceMetadata.Empty))
        {
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return Task.FromResult<IResourceInfo>(new PhysicalDirectoryInfo(uri));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            using (var streamReader = new StreamReader(value))
            {
                var fullName = Path.Combine(uri.Path.Decoded, await streamReader.ReadToEndAsync());
                Directory.CreateDirectory(fullName);
                return await GetAsync(fullName, metadata);
            }
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            Directory.Delete(uri.Path.Decoded, true);
            return await GetAsync(uri, metadata);
        }
    }

    [PublicAPI]
    internal class PhysicalDirectoryInfo : ResourceInfo
    {
        public PhysicalDirectoryInfo([NotNull] UriString uri) : base(uri)
        {
        }

        public override UriString Uri { get; }

        public override bool Exists => Directory.Exists(Uri.Path.Decoded);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? Directory.GetLastWriteTimeUtc(Uri.Path.Decoded) : default;

        protected override Task CopyToAsyncInternal(Stream stream) => throw new NotSupportedException();

        protected override Task<object> DeserializeAsyncInternal(Type targetType) => throw new NotSupportedException();
    }
}