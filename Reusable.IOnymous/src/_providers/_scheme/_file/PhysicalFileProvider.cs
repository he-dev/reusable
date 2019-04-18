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
        public PhysicalFileProvider(Metadata metadata = default)
            : base(metadata) { }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
        {
            ValidateFormatNotNull(this, uri, metadata);

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri, metadata.Resource().Format()));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            ValidateFormatNotNull(this, uri, metadata);

            using (var fileStream = new FileStream(uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            {
                await value.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(uri, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, Metadata metadata)
        {
            File.Delete(uri.ToUnc());
            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }        
    }

    [PublicAPI]
    internal class PhysicalFileInfo : ResourceInfo
    {
        public PhysicalFileInfo([NotNull] UriString uri, MimeType format)
            : base(uri, m => m.Format(format)) { }

        public PhysicalFileInfo([NotNull] UriString uri)
            : this(uri, MimeType.Null) { }

        public override bool Exists => File.Exists(Uri.ToUnc());

        public override long? Length => new FileInfo(Uri.ToUnc()).Length;

        public override DateTime? CreatedOn => Exists ? File.GetCreationTimeUtc(Uri.ToUnc()) : default;

        public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc(Uri.ToUnc()) : default;

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            using (var fileStream = File.OpenRead(Uri.ToUnc()))
            {
                await fileStream.CopyToAsync(stream);
            }
        }
    }
}