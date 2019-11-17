using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IResourceController> ResourceControllerFilterCallback(IEnumerable<IResourceController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByControllerTags(this IEnumerable<IResourceController> providers, Request request)
        {
            // The request doesn't specify any tags.
            if (!request.Metadata.GetItemOrDefault(ResourceController.Tags).Any())
            {
                return providers;
            }
            
            return
                from p in providers
                let providerTags = p.Properties.GetItemOrDefault(ResourceController.Tags)
                where providerTags.Overlaps(request.Metadata.GetItemOrDefault(ResourceController.Tags))
                select p;
        }

        public static IEnumerable<IResourceController> FilterByUriScheme(this IEnumerable<IResourceController> controllers, Request request)
        {
            // There is nothing to filter by as the request uses a relative Uri.
            if (request.Uri.IsRelative)
            {
                return controllers;
            }
            
            return
                from c in controllers
                where c.Properties.GetItem(ResourceController.Schemes).Contains(request.Uri.Scheme)
                select c;
        }

        public static IEnumerable<IResourceController> FilterByUriPath(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.Uri.IsAbsolute)
            {
                return controllers;
            }

            return
                from c in controllers
                where c.SupportsRelativeUri()
                select c;
        }
    }
}