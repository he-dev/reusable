using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IResourceController> ResourceControllerFilterCallback(IEnumerable<IResourceController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByControllerName(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.ControllerName.Equals(ControllerName.Empty))
            {
                return
                    from c in controllers
                    where c.ControllerName.Tags.Overlaps(request.ControllerName.Tags)
                    select c;
            }
            else
            {
                return
                    from c in controllers
                    where c.ControllerName.Equals(request.ControllerName)
                    select c;
            }
        }

        public static IEnumerable<IResourceController> FilterByRequest(this IEnumerable<IResourceController> controllers, Request request)
        {
            return
                from c in controllers
                where c.GetType().GetCustomAttribute<HandlesAttribute>().Type.IsInstanceOfType(request)
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