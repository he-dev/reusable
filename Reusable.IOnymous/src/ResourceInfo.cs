using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceInfo : IDisposable, IEquatable<IResourceInfo>, IEquatable<string>
    {
        [NotNull]
        UriString Uri { get; }

        bool Exists { get; }

        long? Length { get; }

        DateTime? CreatedOn { get; }

        DateTime? ModifiedOn { get; }

        Metadata Metadata { get; }

        Task CopyToAsync(Stream stream);
    }

    [PublicAPI]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceInfo : IResourceInfo
    {
        protected ResourceInfo([NotNull] UriString uri, ConfigureMetadataScopeCallback<IResourceInfo> configureMetadata)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            Uri = uri.IsRelative ? new UriString($"{ResourceProvider.DefaultScheme}:{uri}") : uri;
            Metadata = Metadata.Empty.Resource(configureMetadata);
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

        public virtual MimeType Format => Metadata.Resource().Format();

        public virtual Metadata Metadata { get; }

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

        public override bool Equals(object obj) => obj is IResourceInfo resource && Equals(resource);

        public bool Equals(IResourceInfo other) => ResourceInfoEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceInfoEqualityComparer.Default.Equals((UriString)other, Uri);

        public override int GetHashCode() => ResourceInfoEqualityComparer.Default.GetHashCode(this);

        #endregion

        #region Helpers

        #endregion

        public virtual void Dispose() { }
    }
}