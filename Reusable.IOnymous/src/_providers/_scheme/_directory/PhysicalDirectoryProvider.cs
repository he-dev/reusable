using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalDirectoryProvider : ResourceProvider
    {
        public PhysicalDirectoryProvider(ImmutableSession metadata = default)
            : base(new SoftString[] { "directory" }, metadata) { }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return Task.FromResult<IResourceInfo>(new PhysicalDirectoryInfo(uri));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            using (var streamReader = new StreamReader(value))
            {
                var fullName = Path.Combine(uri.Path.Decoded.ToString(), await streamReader.ReadToEndAsync());
                Directory.CreateDirectory(fullName);
                return await GetAsync(fullName, metadata);
            }
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            Directory.Delete(uri.Path.Decoded.ToString(), true);
            return await GetAsync(uri, metadata);
        }
    }

    [PublicAPI]
    internal class PhysicalDirectoryInfo : ResourceInfo
    {
        public PhysicalDirectoryInfo([NotNull] UriString uri)
            : base(uri, ImmutableSession.Empty.Set(Use<IResourceNamespace>.Namespace, x => x.Format, MimeType.Null)) { }

        public override UriString Uri { get; }

        public override bool Exists => Directory.Exists(Uri.Path.Decoded.ToString());

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? Directory.GetLastWriteTimeUtc(Uri.Path.Decoded.ToString()) : default;

        protected override Task CopyToAsyncInternal(Stream stream) => throw new NotSupportedException();
    }
}