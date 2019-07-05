using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalDirectoryProvider : ResourceProvider
    {
        public PhysicalDirectoryProvider(IImmutableSession properties = default)
            : base(properties.ThisOrEmpty().SetScheme("directory"))
        {
            Methods =
                MethodDictionary
                    .Empty
                    .Add(ResourceRequestMethod.Get, GetAsync)
                    .Add(ResourceRequestMethod.Put, PutAsync)
                    .Add(ResourceRequestMethod.Delete, DeleteAsync);
        }

        private Task<IResource> GetAsync(ResourceRequest request)
        {
            return Task.FromResult<IResource>(new PhysicalDirectory(request.Uri));
        }

        private async Task<IResource> PutAsync(ResourceRequest request)
        {
            using (var streamReader = new StreamReader(request.Body))
            {
                var fullName = Path.Combine(request.Uri.Path.Decoded.ToString(), await streamReader.ReadToEndAsync());
                Directory.CreateDirectory(fullName);
                return await GetAsync(new ResourceRequest { Uri = fullName });
            }
        }

        private async Task<IResource> DeleteAsync(ResourceRequest request)
        {
            Directory.Delete(request.Uri.Path.Decoded.ToString(), true);
            return await GetAsync(request);
        }
    }

    [PublicAPI]
    internal class PhysicalDirectory : Resource
    {
        public PhysicalDirectory([NotNull] UriString uri)
            : base(uri, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.Format), MimeType.None)) { }

        public override UriString Uri { get; }

        public override bool Exists => Directory.Exists(Uri.Path.Decoded.ToString());

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? Directory.GetLastWriteTimeUtc(Uri.Path.Decoded.ToString()) : default;

        protected override Task CopyToAsyncInternal(Stream stream) => throw new NotSupportedException();
    }
}