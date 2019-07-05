using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResource : IDisposable, IEquatable<IResource>, IEquatable<string>
    {
        [NotNull]
        UriString Uri { get; }

        bool Exists { get; }

        long? Length { get; }

        DateTime? CreatedOn { get; }

        DateTime? ModifiedOn { get; }

        IImmutableSession Metadata { get; }

        Task CopyToAsync(Stream stream);
    }

    [PublicAPI]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class Resource : IResource
    {
        protected Resource
        (
            [NotNull] UriString uri,
            [NotNull] IImmutableSession metadata
        )
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            Uri = uri.IsRelative ? new UriString($"{ResourceSchemes.IOnymous}:{uri}") : uri;
            Metadata = metadata;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Uri);
            builder.DisplayValue(x => x.Exists);
            builder.DisplayValue(x => x.Format);
        });

        #region IResourceInfo

        public virtual UriString Uri { get; }

        public abstract bool Exists { get; }

        public abstract long? Length { get; }

        public abstract DateTime? CreatedOn { get; }

        public abstract DateTime? ModifiedOn { get; }

        public virtual MimeType Format => Metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Format));

        public virtual IImmutableSession Metadata { get; }

        #endregion

        #region Wrappers

        // These wrappers are to provide helpful exceptions.

        public async Task CopyToAsync(Stream stream)
        {
            if (!Exists)
            {
                throw new InvalidOperationException($"Resource '{Uri}' does not exist.");
            }

            try
            {
                await CopyToAsyncInternal(stream);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    "Resource",
                    $"An error occured while trying to copy the '{Uri}'. See the inner exception for details.",
                    inner
                );
            }
        }

        #endregion

        #region Internal

        protected abstract Task CopyToAsyncInternal(Stream stream);

        #endregion

        #region IEquatable<IResourceInfo>

        public override bool Equals(object obj) => obj is IResource resource && Equals(resource);

        public bool Equals(IResource other) => ResourceEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceEqualityComparer.Default.Equals((UriString)other, Uri);

        public override int GetHashCode() => ResourceEqualityComparer.Default.GetHashCode(this);

        #endregion

        #region Helpers

        #endregion

        public virtual void Dispose() { }
    }
}