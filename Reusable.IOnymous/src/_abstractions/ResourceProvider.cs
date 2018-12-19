using System;
using System.IO;
using System.Runtime.CompilerServices;
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

        public virtual SoftString Scheme => (SoftString)Metadata[ResourceMetadataKeys.Scheme];

        public abstract Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null);

        public virtual Task<IResourceInfo> PostAsync(UriString name, Stream value, ResourceMetadata metadata = null)
        {
            throw new NotImplementedException();
        }

        public abstract Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null);

        public abstract Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null);

        protected static Exception CreateException(IResourceProvider provider, string name, ResourceMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            return new Exception();
        }

        protected UriString ValidateScheme([NotNull] UriString uri, string scheme)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            return
                SoftString.Comparer.Equals(uri.Scheme, scheme)
                    ? uri
                    : throw DynamicException.Create("InvalidScheme", $"This resource-provider '{GetType().ToPrettyString()}' requires scheme '{scheme}'.");
        }

        protected UriString ValidateSchemeNotEmpty([NotNull] UriString uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            return
                uri.Scheme
                    ? uri
                    : throw DynamicException.Create("SchemeNotFound", $"Uri '{uri}' does not contain scheme.");
        }
    }
}