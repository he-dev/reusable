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
            : base(provider.Properties.SetItem(PropertySelector.Select(x => x.AllowRelativeUri), true))
        {
            Methods = provider.Methods.Aggregate(MethodDictionary.Empty, (current, next) =>
            {
                return current.Add(next.Key, request => next.Value(new Request
                {
                    Uri = Resolve(request.Uri),
                    Method = request.Method,
                    Properties = request.Properties,
                    CreateBodyStreamFunc = request.CreateBodyStreamFunc,
                }));
            });
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