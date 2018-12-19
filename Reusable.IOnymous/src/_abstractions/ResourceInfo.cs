using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionizer;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceInfo : IEquatable<IResourceInfo>, IEquatable<string>
    {
        [NotNull]
        UriString Uri { get; }

        bool Exists { get; }

        long? Length { get; }

        DateTime? CreatedOn { get; }

        DateTime? ModifiedOn { get; }

        Task CopyToAsync(Stream stream);

        Task<object> DeserializeAsync(Type targetType);
    }

    [PublicAPI]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceInfo : IResourceInfo
    {
        protected const int OutOfRange = -1;

        protected ResourceInfo([NotNull] UriString uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Uri = uri.IsRelative ? new UriString($"{ResourceProvider.DefaultScheme}:{uri}") : uri;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x.Uri);
            builder.Property(x => x.Exists);
        });

        #region IResourceInfo

        public virtual UriString Uri { get; }

        public abstract bool Exists { get; }

        public abstract long? Length { get; }

        public abstract DateTime? CreatedOn { get; }

        public abstract DateTime? ModifiedOn { get; }

        #endregion

        #region Wrappers
        
        // These wrappers are to provide helpful exceptions.

        public async Task CopyToAsync(Stream stream)
        {
            AssertExists();

            try
            {
                await CopyToAsyncInternal(stream);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"{nameof(CopyToAsync)}",
                    $"Affected resource '{Uri}'.",
                    inner
                );
            }
        }

        public async Task<object> DeserializeAsync(Type targetType)
        {
            AssertExists();

            try
            {
                return await DeserializeAsyncInternal(targetType);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"{nameof(CopyToAsync)}",
                    $"Affected resource '{Uri}'.",
                    inner
                );
            }
        }

        #endregion

        #region Internal

        protected abstract Task CopyToAsyncInternal(Stream stream);

        protected abstract Task<object> DeserializeAsyncInternal(Type targetType);

        #endregion

        #region IEquatable<IResourceInfo>

        public override bool Equals(object obj) => obj is IResourceInfo resource && Equals(resource);

        public bool Equals(IResourceInfo other) => ResourceInfoEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceInfoEqualityComparer.Default.Equals((UriString)other, Uri);

        public override int GetHashCode() => ResourceInfoEqualityComparer.Default.GetHashCode(this);

        #endregion

        #region Helpers

        protected void AssertExists([CallerMemberName] string memberName = null)
        {
            if (!Exists)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                throw DynamicException.Create(memberName, $"Resource '{Uri}' does not exist.");
            }
        }

        #endregion
    }
}