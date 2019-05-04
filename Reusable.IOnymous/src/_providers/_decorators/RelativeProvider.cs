using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public class RelativeProvider : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        private readonly UriString _baseUri;

        public RelativeProvider([NotNull] IResourceProvider resourceProvider, [NotNull] UriString baseUri)
            : base(resourceProvider.Schemes, resourceProvider.Metadata.Set(Use<IProviderSession>.Namespace,x => x.AllowRelativeUri, true))
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            //if (baseUri.IsAbsolute) throw new ArgumentException($"'{nameof(baseUri)}' must be relative.");
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }
        
        public static Func<IResourceProvider, RelativeProvider> Factory(UriString baseUri)
        {
            return decorable => new RelativeProvider(decorable, baseUri);
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return await _resourceProvider.GetAsync(Combine(uri), metadata);
        }

        protected override Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return _resourceProvider.PostAsync(Combine(uri), value, metadata);
        }
        
        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return _resourceProvider.PutAsync(Combine(uri), value, metadata);
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return _resourceProvider.DeleteAsync(Combine(uri), metadata);
        }

        private UriString Combine(UriString uri)
        {
            return uri.IsRelative ? _baseUri + uri : uri;
        }
    }
}