using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class RelativeProvider : ResourceProvider
    {

        private readonly UriString _baseUri;

        public RelativeProvider([NotNull] UriString baseUri, [NotNull] IResourceProvider provider)
            : base(provider.Properties.SetItem(PropertySelector.Select(x => x.AllowRelativeUri), true))
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));

            Methods = provider.Methods.Aggregate(MethodDictionary.Empty, (current, next) =>
            {
                return current.Add(next.Key, request => next.Value(new ResourceRequest
                {
                    Uri = Prefix(request.Uri),
                    Body = request.Body,
                    Method = request.Method,
                    Properties = request.Properties
                }));
            });
        }

        public static Func<IResourceProvider, RelativeProvider> Factory(UriString baseUri)
        {
            return decorable => new RelativeProvider(baseUri, decorable);
        }

        private UriString Prefix(UriString uri)
        {
            return uri.IsRelative ? _baseUri + uri : uri;
        }
    }
}