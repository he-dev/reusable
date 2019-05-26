using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public abstract class MailProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "mailto";

        protected MailProvider(ImmutableSession metadata) : base(new SoftString[] { DefaultScheme }, metadata) { }

        protected async Task<string> ReadBodyAsync(Stream value, IImmutableSession metadata)
        {
            using (var bodyReader = new StreamReader(value, metadata.GetItemOrDefault(From<IMailMeta>.Select(x => x.BodyEncoding), Encoding.UTF8)))
            {
                return await bodyReader.ReadToEndAsync();
            }
        }
    }

    [TypeMemberKeyFactory]
    [TrimEnd("I")]
    [TrimStart("Meta")]
    public interface IMailMeta : INamespace
    {
        string From { get; }

        IList<string> To { get; }

        [CanBeNull]
        // ReSharper disable once InconsistentNaming - We want to keep this particular name as is.
        IList<string> CC { get; }

        string Subject { get; }

        [CanBeNull]
        IDictionary<string, byte[]> Attachments { get; }

        Encoding BodyEncoding { get; }

        bool IsHtml { get; }

        bool IsHighPriority { get; }
    }

    internal class MailResourceInfo : ResourceInfo
    {
        private readonly Stream _response;

        public MailResourceInfo([NotNull] UriString uri, Stream response, MimeType format)
            : base(uri, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.Format), format))
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