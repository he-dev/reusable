using System.Collections.Generic;
using System.Linq;

namespace Reusable.IOnymous
{
    public delegate IEnumerable<IResourceProvider> ResourceProviderFilterCallback(IEnumerable<IResourceProvider> providers, Request request);
    
    public static class ResourceProviderFilters
    {
        public static IEnumerable<IResourceProvider> FilterByName(this IEnumerable<IResourceProvider> providers, Request request)
        {
            var canFilter = request.Context.GetNames().Any();
            return providers.Where(p => !canFilter || p.Properties.GetNames().Overlaps(request.Context.GetNames()));
        }

        public static IEnumerable<IResourceProvider> FilterByScheme(this IEnumerable<IResourceProvider> providers, Request request)
        {
            var canFilter = !(request.Uri.IsRelative || (request.Uri.IsAbsolute && request.Uri.Scheme == UriSchemes.Custom.IOnymous));
            return providers.Where(p => !canFilter || p.Properties.GetSchemes().Overlaps(new[] { UriSchemes.Custom.IOnymous, request.Uri.Scheme }));
        }
    }
}