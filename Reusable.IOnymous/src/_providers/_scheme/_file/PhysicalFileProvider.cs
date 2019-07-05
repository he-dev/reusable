using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : ResourceProvider
    {
        private static readonly IExpressValidator<Request> RequestValidator = ExpressValidator.For<Request>(builder =>
        {
            builder.False
            (x =>
                x.Metadata.GetItemOrDefault(From<IResourceMeta>.Select(y => y.Format), MimeType.None).IsNull()
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it requires resource format specified by the metadata.");
        });

        public PhysicalFileProvider(IImmutableSession properties = default)
            : base(properties.ThisOrEmpty().SetScheme("file"))
        {
            Methods.Add(ResourceRequestMethod.Get, GetAsync);
            Methods.Add(ResourceRequestMethod.Get, PutAsync);
            Methods.Add(ResourceRequestMethod.Get, DeleteAsync);
        }

        private Task<IResource> GetAsync(ResourceRequest request)
        {
            return Task.FromResult<IResource>(
                new PhysicalFile(
                    request.Uri,
                    request.Headers.GetItemOrDefault(From<IResourceMeta>.Select(y => y.Format))));
        }
        
        private async Task<IResource> PutAsync(ResourceRequest request)
        {
            using (var fileStream = new FileStream(request.Uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            {
                await request.Body.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(request);
        }
        
        private Task<IResource> DeleteAsync(ResourceRequest request)
        {
            File.Delete(request.Uri.ToUnc());
            return Task.FromResult<IResource>(new PhysicalFile(request.Uri));
        }

        protected override Task<IResource> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            ValidateRequest(ExtractMethodName(nameof(GetAsync)), uri, metadata, Stream.Null, RequestValidator);

            return Task.FromResult<IResource>(new PhysicalFile(uri, metadata.GetItemOrDefault(From<IResourceMeta>.Select(y => y.Format))));
        }

        protected override async Task<IResource> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            ValidateRequest(ExtractMethodName(nameof(PutAsync)), uri, metadata, Stream.Null, RequestValidator);

            using (var fileStream = new FileStream(uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            {
                await value.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(uri, metadata);
        }

        protected override Task<IResource> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            File.Delete(uri.ToUnc());
            return Task.FromResult<IResource>(new PhysicalFile(uri));
        }
    }

    [PublicAPI]
    internal class PhysicalFile : Resource
    {
        public PhysicalFile([NotNull] UriString uri, MimeType format)
            : base(uri, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.Format), format)) { }

        public PhysicalFile([NotNull] UriString uri)
            : this(uri, MimeType.None) { }

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