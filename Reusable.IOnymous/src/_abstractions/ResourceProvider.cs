using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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
    using static ResourceMetadataKeys;

    [PublicAPI]
    public interface IResourceProvider
    {
        [NotNull]
        ResourceMetadata Metadata { get; }
        
        bool CanGet { get; }
        bool CanPost { get; }
        bool CanPut { get; }
        bool CanDelete { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, ResourceMetadata metadata = null);
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly ImplicitString DefaultScheme = "ionymous";

        protected ResourceProvider([NotNull] ResourceMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            if (!metadata.ContainsKey(ResourceMetadataKeys.SchemeSet)) throw new ArgumentException(paramName: nameof(metadata), message: $"Resource provider metadata must specify the scheme.");

            // If this is a decorator then the decorated resource-provider already has set this.
            if (!metadata.ContainsKey(ProviderDefaultName))
            {
                metadata = metadata.Add(ProviderDefaultName, GetType().ToPrettyString());
            }

            Metadata = metadata;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayCollection(x => x.Metadata.ProviderNames());
            builder.DisplayMember(x => x.SchemeSet);
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

        public virtual IImmutableSet<SoftString> SchemeSet => (IImmutableSet<SoftString>)Metadata[ResourceMetadataKeys.SchemeSet];

        #region Wrappers

        // These wrappers are to provide helpful exceptions.

        public async Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await GetAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PostAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await PostAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await PutAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

            try
            {
                return await DeleteAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(uri, metadata, inner);
            }
        }

        #endregion

        #region Internal

        protected virtual Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null) => throw MethodNotSupportedException(uri);

        #endregion

        #region Helpers

        protected Exception CreateException(UriString uri, ResourceMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var method = Regex.Replace(memberName, "Async$", string.Empty).ToUpper();

            throw DynamicException.Create
            (
                memberName,
                $"{GetType().ToPrettyString()} was unable to perform {method} for the given resource '{uri}'.",
                inner
            );
        }

        #endregion

        #region Validations

        protected void ValidateSchemeMatches([NotNull] UriString uri)
        {
            if (Metadata.TryGetValue(AllowRelativeUri, out bool allow) && allow)
            {
                return;
            }

            if (SchemeSet.Contains(DefaultScheme))
            {
                return;
            }

            if (!SchemeSet.Contains(uri.Scheme))
            {
                throw DynamicException.Create
                (
                    "InvalidScheme",
                    $"{GetType().ToPrettyString()} requires scheme '{SchemeSet}'."
                );
            }
        }

        protected Exception MethodNotSupportedException(UriString uri, [CallerMemberName] string memberName = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var method = Regex.Match(memberName, "^(?<method>)Async").Groups["method"].Value;

            return DynamicException.Create
            (
                $"{method}NotSupported",
                $"Cannot '{method.ToUpper()}' at '{uri}' because '{GetType().ToPrettyString()}' doesn't support it."
            );
        }

        protected void ValidateSchemeNotEmpty([NotNull] UriString uri)
        {
            if (Metadata.TryGetValue(AllowRelativeUri, out bool allow) && allow)
            {
                return;
            }

            if (!uri.Scheme)
            {
                throw DynamicException.Create("SchemeNotFound", $"Uri '{uri}' does not contain scheme.");
            }
        }

        #endregion
    }
}