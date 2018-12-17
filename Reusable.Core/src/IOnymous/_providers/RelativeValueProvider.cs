using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public partial class RelativeResourceProvider : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        private readonly SimpleUri _baseUri;

        public RelativeResourceProvider([NotNull] IResourceProvider resourceProvider, [NotNull] SimpleUri baseUri)
            : base(resourceProvider.Metadata)
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        public override async Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            return await _resourceProvider.GetAsync(CreateAbsoluteUri(uri));
        }

        public override Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(CreateAbsoluteUri(uri), value, metadata);
        }

        public override Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(uri, metadata);
        }

        private SimpleUri CreateAbsoluteUri(SimpleUri uri) => uri.IsRelative ? (SimpleUri)(_baseUri.Path + "/" + uri.Path) : uri;
    }
}