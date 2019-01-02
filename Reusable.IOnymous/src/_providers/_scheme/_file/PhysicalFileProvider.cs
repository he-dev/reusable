using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : FileProvider
    {
        public PhysicalFileProvider(ResourceMetadata metadata = default)
            : base(metadata)
        {
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata)
        {
            ValidateFormatNotNull(this, uri, metadata);

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri, metadata.Format()));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata)
        {
            ValidateFormatNotNull(this, uri, metadata);
            
            using (var fileStream = new FileStream(uri.Path.Decoded.ToString(), FileMode.CreateNew, FileAccess.Write))
            {
                await value.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(uri, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata)
        {
            File.Delete((string)uri.Path.Decoded);
            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }
    }

    [PublicAPI]
    internal class PhysicalFileInfo : ResourceInfo
    {
        public PhysicalFileInfo([NotNull] UriString uri, MimeType format)
            : base(uri, format)
        {
        }

        public PhysicalFileInfo([NotNull] UriString uri)
            : this(uri, MimeType.Null)
        {
        }

        public override bool Exists => File.Exists((string)Uri.Path.Decoded);

        public override long? Length => new FileInfo((string)Uri.Path.Decoded).Length;

        public override DateTime? CreatedOn => Exists ? File.GetCreationTimeUtc((string)Uri.Path.Decoded) : default;

        public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc((string)Uri.Path.Decoded) : default;

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            using (var fileStream = File.OpenRead((string)Uri.Path.Decoded))
            {
                await fileStream.CopyToAsync(stream);
            }
        }
    }
}