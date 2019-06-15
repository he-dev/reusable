using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class EnvironmentVariableProvider : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        public EnvironmentVariableProvider([NotNull] IResourceProvider resourceProvider)
            : base(resourceProvider.Schemes, resourceProvider.Metadata.SetItem(From<IProviderMeta>.Select(x => x.AllowRelativeUri), true))
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        }

        public static Func<IResourceProvider, EnvironmentVariableProvider> Factory()
        {
            return decorable => new EnvironmentVariableProvider(decorable);
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return _resourceProvider.GetAsync(UpdatePath(uri), metadata);
        }

        protected override Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return _resourceProvider.PostAsync(UpdatePath(uri), value, metadata);
        }
        
        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return _resourceProvider.PutAsync(UpdatePath(uri), value, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return _resourceProvider.DeleteAsync(UpdatePath(uri), metadata);
        }

        private UriString UpdatePath(UriString uri)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(uri.Path.Decoded.ToString());
            var normalizedPath = UriStringHelper.Normalize(expandedPath);
            uri = uri.With(x => x.Path, new UriStringComponent(normalizedPath));
            if (!uri.Scheme && Path.IsPathRooted(uri.Path.Decoded.ToString()))
            {
                uri = uri.With(x => x.Scheme, "file");
            }

            return uri;
        }
    }
}