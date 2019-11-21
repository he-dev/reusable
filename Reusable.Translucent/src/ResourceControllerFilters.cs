using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IResourceController> ResourceControllerFilterCallback(IEnumerable<IResourceController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByControllerTags(this IEnumerable<IResourceController> controllers, Request request)
        {
            // The request doesn't specify any tags.
            if (!request.ControllerTags.Any())
            {
                return controllers;
            }

            return
                from p in controllers
                let providerTags = p.Tags
                where providerTags.Overlaps(request.ControllerTags)
                select p;
        }

        public static IEnumerable<IResourceController> FilterByControllerId(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.ControllerId is {})
            {
                return
                    from c in controllers
                    where c.Id?.Equals(request.ControllerId) == true
                    select c;
            }

            return controllers;
        }

        public static IEnumerable<IResourceController> FilterByUriScheme(this IEnumerable<IResourceController> controllers, Request request)
        {
            // There is nothing to filter by as the request uses a relative Uri.
            var schemes = request.Uri.IsAbsolute ? new[] { request.Uri.Scheme }.AsEnumerable() : request.GetType().GetCustomAttribute<SchemeAttribute>();

            return
                from c in controllers
                where c.Schemes.Overlaps(schemes)
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
                where c.SupportsRelativeUri
                select c;
        }
    }
}