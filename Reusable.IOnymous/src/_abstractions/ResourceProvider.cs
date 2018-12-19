using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, ResourceMetadata metadata = null);
    }

    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly string DefaultScheme = "ionymous";

        protected ResourceProvider(ResourceMetadata metadata)
        {
            if (!metadata.ContainsKey(ResourceMetadataKeys.Scheme)) throw new ArgumentException(paramName: nameof(metadata), message: $"Resource provider metadata must specify the scheme.");

            // If this is a decorator then the decorated resource-provider already has set this.
            if (!metadata.ContainsKey(ProviderDefaultName))
            {
                metadata = metadata.Add(ProviderDefaultName, GetType().ToPrettyString());
            }

            Metadata = metadata;
        }

        public virtual ResourceMetadata Metadata { get; }

        public virtual SoftString Scheme => (SoftString)(string)Metadata[ResourceMetadataKeys.Scheme];

        public async Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);
            ValidateCanMethod();

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
            ValidateCanMethod();

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
            ValidateCanMethod();

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
            ValidateCanMethod();

            try
            {
                return await DeleteAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(uri, metadata, inner);
            }
        }

        #region Internal

        protected virtual Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null) => throw new NotSupportedException();

        protected virtual Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null) => throw new NotSupportedException();

        protected virtual Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null) => throw new NotSupportedException();

        protected virtual Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null) => throw new NotSupportedException();

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

            if (SoftString.Comparer.Equals(Scheme, DefaultScheme))
            {
                return;
            }
            
            if (!SoftString.Comparer.Equals(uri.Scheme, Scheme))
            {
                throw DynamicException.Create
                (
                    "InvalidScheme",
                    $"{GetType().ToPrettyString()} requires scheme '{Scheme}'."
                );
            }
        }

        protected void ValidateCanMethod([CallerMemberName] string memberName = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var method = Regex.Replace(memberName, "Async$", string.Empty);

            if (!Metadata.TryGetValue($"Can{method}", out bool can) || !can)
            {
                throw DynamicException.Create
                (
                    $"{method}NotSupported",
                    $"{GetType().ToPrettyString()} doesn't support '{method.ToUpper()}'."
                );
            }
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