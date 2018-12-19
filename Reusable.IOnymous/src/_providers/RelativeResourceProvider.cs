using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public partial class RelativeResourceProvider : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        private readonly UriString _baseUri;

        public RelativeResourceProvider([NotNull] IResourceProvider resourceProvider, [NotNull] UriString baseUri)
            : base(resourceProvider.Metadata.Add(ResourceMetadataKeys.AllowRelativeUri, true))
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return await _resourceProvider.GetAsync(CreateAbsoluteUri(uri));
        }

        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(CreateAbsoluteUri(uri), value, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(uri, metadata);
        }

        private UriString CreateAbsoluteUri(UriString uri) => uri.IsRelative ? (UriString)(_baseUri + uri.Path) : uri;
    }
}