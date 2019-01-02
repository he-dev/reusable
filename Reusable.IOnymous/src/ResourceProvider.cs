using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceProvider : IDisposable
    {
        [NotNull]
        ResourceMetadata Metadata { get; }

        IImmutableSet<SoftString> Schemes { get; }

        bool CanGet { get; }

        bool CanPost { get; }

        bool CanPut { get; }

        bool CanDelete { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, ResourceMetadata metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, ResourceMetadata metadata = default);
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly SoftString DefaultScheme = "ionymous";

        protected ResourceProvider([NotNull] IEnumerable<SoftString> schemes, ResourceMetadata metadata)
        {
            if (schemes == null) throw new ArgumentNullException(nameof(schemes));

            // If this is a decorator then the decorated resource-provider already has set this.
            if (!metadata.ProviderDefaultName())
            {
                metadata = metadata.ProviderDefaultName(GetType().ToPrettyString());
            }

            Schemes = schemes.ToImmutableHashSet();

            if (Schemes.Empty()) throw new ArgumentException(paramName: nameof(metadata), message: $"{nameof(schemes)} must not be empty.");

            Metadata = metadata;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayCollection(x => x.Metadata.ProviderNames());
            builder.DisplayMember(x => x.Schemes);
        });

        public bool CanGet => Implements(nameof(GetAsyncInternal));

        public bool CanPost => Implements(nameof(PostAsyncInternal));

        public bool CanPut => Implements(nameof(PutAsyncInternal));

        public bool CanDelete => Implements(nameof(DeleteAsyncInternal));

        private bool Implements(string methodName)
        {
            // ReSharper disable once PossibleNullReferenceException - nope, not true in this case
            return GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType == GetType();
        }

        public virtual ResourceMetadata Metadata { get; }

        public virtual IImmutableSet<SoftString> Schemes { get; }

        #region Wrappers

        // These wrappers are to provide helpful exceptions.        

        public async Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = default)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await GetAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PostAsync(UriString uri, Stream value, ResourceMetadata metadata = default)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await PostAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = default)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await PutAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = default)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await DeleteAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        #endregion

        #region Internal

        protected virtual Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata) => throw MethodNotSupportedException(uri);

        #endregion

        #region Validations

        protected void ValidateSchemeMatches([NotNull] UriString uri, [CallerMemberName] string memberName = null)
        {
            if (Metadata.AllowRelativeUri())
            {
                return;
            }

            if (Schemes.Contains(DefaultScheme))
            {
                return;
            }

            if (!Schemes.Contains(uri.Scheme))
            {
                throw DynamicException.Create
                (
                    "InvalidScheme",
                    Because(memberName, uri, $"it requires uri to specify scheme [{Schemes.Join(",")}].")
                );
            }
        }

        protected void ValidateSchemeNotEmpty([NotNull] UriString uri, [CallerMemberName] string memberName = null)
        {
            if (Metadata.AllowRelativeUri())
            {
                return;
            }

            if (!uri.Scheme)
            {
                throw DynamicException.Create
                (
                    "MissingScheme",
                    Because(memberName, uri, "uri it must contain scheme.")
                );
            }
        }

        protected void ValidateFormatNotNull<T>(T fileProvider, UriString uri, ResourceMetadata metadata, [CallerMemberName] string memberName = null) where T : IResourceProvider
        {
            if (metadata.Format().IsNull)
            {
                throw new ArgumentException
                (
                    paramName: nameof(metadata),
                    message: ResourceHelper.FormatMessage<T>(memberName, uri, $"you need to specify file format via {nameof(metadata)}.")
                );
            }
        }

        protected Exception MethodNotSupportedException(UriString uri, [CallerMemberName] string memberName = null)
        {
            return DynamicException.Create
            (
                $"{ExtractMethodName(memberName)}NotSupported",
                Because(memberName, uri, "it doesn't support it.")
            );
        }

        #endregion

        #region Helpers

        protected Exception WrapException(UriString uri, ResourceMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            throw DynamicException.Create
            (
                ExtractMethodName(memberName),
                Because(memberName, uri, "of an error. See inner exception for details."),
                inner
            );
        }

        private string ExtractMethodName(string memberName)
        {
            return Regex.Match(memberName, @"^(?<method>\w+)Async").Groups["method"].Value;
        }

        private string Because(string memberName, UriString uri, string reason)
        {
            return $"{GetType().ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        }

        #endregion

        public virtual void Dispose()
        {
            // Can be overriden when derived.
        }

        #region operators

        //public static ResourceProvider operator +(ResourceProvider decorable, Func<ResourceProvider, ResourceProvider> decorator) => decorator(decorable);

        #endregion
    }
}