using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public abstract class MailProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "mailto";

        protected MailProvider(Metadata metadata) : base(new SoftString[] { DefaultScheme }, metadata) { }

        protected async Task<string> ReadBodyAsync(Stream value, Metadata metadata)
        {
            using (var bodyReader = new StreamReader(value, metadata.Mail().BodyEncoding()))
            {
                return await bodyReader.ReadToEndAsync();
            }
        }
    }

    internal class MailResourceInfo : ResourceInfo
    {
        private readonly Stream _response;

        public MailResourceInfo([NotNull] UriString uri, Stream response, MimeType format)
            : base(uri, m => m.Format(format))
        {
            _response = response;
        }

        public override bool Exists => !(_response is null);

        public override long? Length => _response?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            await _response.Rewind().CopyToAsync(stream);
        }

        public override void Dispose()
        {
            _response.Dispose();
        }
    }
}