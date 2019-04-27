using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : FileProvider
    {
        private static readonly IExpressValidator<Request> RequestValidator = ExpressValidator.For<Request>(builder =>
        {
            builder.False
            (x =>
                x.Metadata.Get(Use<IResourceSession>.Scope, y => y.Format, MimeType.Null).IsNull()
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it requires resource format specified by the metadata.");
        });

        public PhysicalFileProvider(IImmutableSession metadata = default)
            : base(metadata.ThisOrEmpty()) { }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            ValidateRequest(ExtractMethodName(nameof(GetAsync)), uri, metadata, Stream.Null, RequestValidator);

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri, metadata.Get(Use<IResourceSession>.Scope, y => y.Format)));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            ValidateRequest(ExtractMethodName(nameof(PutAsync)), uri, metadata, Stream.Null, RequestValidator);

            using (var fileStream = new FileStream(uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            {
                await value.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(uri, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            File.Delete(uri.ToUnc());
            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }
    }

    [PublicAPI]
    internal class PhysicalFileInfo : ResourceInfo
    {
        public PhysicalFileInfo([NotNull] UriString uri, MimeType format)
            : base(uri, ImmutableSession.Empty.Set(Use<IResourceSession>.Scope, x => x.Format, format)) { }

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