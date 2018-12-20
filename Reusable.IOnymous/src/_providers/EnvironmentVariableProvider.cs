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
            : base(resourceProvider.Metadata.SetItem(ResourceMetadataKeys.Scheme, "file"))
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.GetAsync(Environment.ExpandEnvironmentVariables(uri.Path.Decoded), metadata);
        }

        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(Environment.ExpandEnvironmentVariables(uri.Path.Decoded), value, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(Environment.ExpandEnvironmentVariables(uri.Path.Decoded), metadata);
        }
    }
}