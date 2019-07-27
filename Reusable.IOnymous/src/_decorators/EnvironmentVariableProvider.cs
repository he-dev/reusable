using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class EnvironmentVariableProvider : ResourceProvider
    {
        public EnvironmentVariableProvider([NotNull] IResourceProvider provider)
            : base(provider.Properties.SetItem(ResourceProviderProperty.AllowRelativeUri, true))
        {
            Methods = provider.Methods.Aggregate
            (
                MethodCollection.Empty,
                (current, next) => current.Add(next.Method, request => next.InvokeCallback(request.Copy(c => c.Uri = Resolve(c.Uri))))
            );
        }

        public static Func<IResourceProvider, EnvironmentVariableProvider> Factory()
        {
            return decorable => new EnvironmentVariableProvider(decorable);
        }

        private UriString Resolve(UriString uri)
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