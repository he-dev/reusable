using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Exceptionizer;

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

        ResourceFormat Format { get; }

        Task CopyToAsync(Stream stream);

        //Task<object> DeserializeAsync(Type targetType);
    }

    [PublicAPI]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceInfo : IResourceInfo
    {
        protected ResourceInfo([NotNull] UriString uri, ResourceFormat format)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Uri = uri.IsRelative ? new UriString($"{ResourceProvider.DefaultScheme}:{uri}") : uri;
            Format = format;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.Uri);
            builder.DisplayMember(x => x.Exists);
        });

        #region IResourceInfo

        public virtual UriString Uri { get; }

        public abstract bool Exists { get; }

        public abstract long? Length { get; }

        public abstract DateTime? CreatedOn { get; }

        public abstract DateTime? ModifiedOn { get; }

        public virtual ResourceFormat Format { get; }

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

        #endregion

        #region Internal

        protected abstract Task CopyToAsyncInternal(Stream stream);

        //protected abstract Task<object> DeserializeAsyncInternal(Type targetType);

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

        public virtual void Dispose()
        {
        }
    }

    public readonly struct ResourceFormat : IEquatable<ResourceFormat>
    {
        public ResourceFormat(string name)
        {
            Name = name;
        }

        [AutoEqualityProperty]
        public SoftString Name { get; }

        public static readonly ResourceFormat Null = new ResourceFormat(nameof(Null));
        public static readonly ResourceFormat String = new ResourceFormat(nameof(String));
        public static readonly ResourceFormat Json = new ResourceFormat(nameof(Json));
        public static readonly ResourceFormat Binary = new ResourceFormat(nameof(Binary));

        public override bool Equals(object obj) => obj is ResourceFormat format && Equals(format);

        public bool Equals(ResourceFormat other) => AutoEquality<ResourceFormat>.Comparer.Equals(this, other);

        public override int GetHashCode() => AutoEquality<ResourceFormat>.Comparer.GetHashCode(this);
    }
}