using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
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

        MimeType Format { get; }
        
        Metadata Metadata { get; }

        Task CopyToAsync(Stream stream);
    }

    [PublicAPI]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceInfo : IResourceInfo
    {
        protected ResourceInfo([NotNull] UriString uri, Metadata metadata = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            Uri = uri.IsRelative ? new UriString($"{ResourceProvider.DefaultScheme}:{uri}") : uri;
            Metadata = metadata;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.Uri);
            builder.DisplayMember(x => x.Exists);
            builder.DisplayMember(x => x.Format);
        });

        #region IResourceInfo

        public virtual UriString Uri { get; }

        public abstract bool Exists { get; }

        public abstract long? Length { get; }

        public abstract DateTime? CreatedOn { get; }

        public abstract DateTime? ModifiedOn { get; }

        public virtual MimeType Format => Metadata.Format();
        
        public virtual Metadata Metadata { get; }

        #endregion

        #region Wrappers

        // These wrappers are to provide helpful exceptions.

        public async Task CopyToAsync(Stream stream)
        {
            var method = ResourceHelper.ExtractMethodName(nameof(CopyToAsync));

            if (!Exists)
            {
                throw DynamicException.Create(method, $"Resource '{Uri}' does not exist.");
            }

            try
            {
                await CopyToAsyncInternal(stream);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    method,
                    $"An error occured while invoking '{method}' for '{Uri}'. See inner exception for details.",
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

        public virtual void Dispose()
        {
        }
    }

    [PublicAPI]
    public readonly struct MimeType : IEquatable<MimeType>
    {
        public MimeType(string name) => Name = name;

        [AutoEqualityProperty]
        public SoftString Name { get; }

        public bool IsNull => this == Null;

        public static readonly MimeType Null = new MimeType(string.Empty);

        /// <summary>
        /// Any document that contains text and is theoretically human readable
        /// </summary>
        public static readonly MimeType Text = new MimeType("text/plain");

        public static readonly MimeType Json = new MimeType("application/json");

        /// <summary>
        /// Any kind of binary data, especially data that will be executed or interpreted somehow.
        /// </summary>
        public static readonly MimeType Binary = new MimeType("application/octet-stream");

        public override bool Equals(object obj) => obj is MimeType format && Equals(format);

        public bool Equals(MimeType other) => AutoEquality<MimeType>.Comparer.Equals(this, other);

        public override int GetHashCode() => AutoEquality<MimeType>.Comparer.GetHashCode(this);

        public override string ToString() => Name.ToString();

        public static bool operator ==(MimeType left, MimeType right) => left.Equals(right);

        public static bool operator !=(MimeType left, MimeType right) => !(left == right);
    }

//    [PublicAPI]
//    public class Identifier : IEquatable<Identifier>
//    {
//        protected Identifier(string name) => Name = name;
//
//        [AutoEqualityProperty]
//        public SoftString Name { get; }
//
//        public static readonly Identifier Null = new Identifier(string.Empty);
//
//        public override bool Equals(object obj) => obj is MimeType format && Equals(format);
//
//        public bool Equals(Identifier other) => AutoEquality<Identifier>.Comparer.Equals(this, other);
//
//        public override int GetHashCode() => AutoEquality<Identifier>.Comparer.GetHashCode(this);
//
//        public override string ToString() => Name.ToString();
//
//        public static bool operator ==(Identifier left, Identifier right) => left?.Equals(right) == true;
//
//        public static bool operator !=(Identifier left, Identifier right) => !(left == right);
//    }
//
//    public class ResourceProviderName : Identifier
//    {
//        public ResourceProviderName(string name) : base(name)
//        {
//        }
//    }
}