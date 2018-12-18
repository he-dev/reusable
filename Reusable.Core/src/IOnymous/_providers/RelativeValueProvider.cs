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
            : base(resourceProvider.Metadata)
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        public override async Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            return await _resourceProvider.GetAsync(CreateAbsoluteUri(uri));
        }

        public override Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(CreateAbsoluteUri(uri), value, metadata);
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(uri, metadata);
        }

        private UriString CreateAbsoluteUri(UriString uri) => uri.IsRelative ? (UriString)(_baseUri.Path + "/" + uri.Path) : uri;
    }
}