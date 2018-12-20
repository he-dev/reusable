using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable.IOnymous
{
    public partial class EnvironmentVariableProvider : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        public EnvironmentVariableProvider([NotNull] IResourceProvider resourceProvider) 
            : base(resourceProvider.Metadata)
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.GetAsync(UpdatePath(uri), metadata);
        }

        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(UpdatePath(uri), value, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(UpdatePath(uri), metadata);
        }

        private UriString UpdatePath(UriString uri)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(uri.Path.Decoded);
            var normalizedPath = UriStringHelper.Normalize(expandedPath);
            return uri.With(x => x.Path, new UriStringComponent(normalizedPath));
        }
    }
}