using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IResourceController> ResourceControllerFilterDelegate(IEnumerable<IResourceController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByController(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.ControllerName.IsNullOrEmpty().Not())
            {
                return
                    from c in controllers
                    where request.ControllerName.Equals(c.Name)
                    select c;
            }

            if (request.ControllerTags.Any())
            {
                return
                    from c in controllers
                    where request.ControllerTags.Overlaps(c.Tags)
                    select c;
            }

            return controllers;
        }

        public static IEnumerable<IResourceController> FilterByRequest(this IEnumerable<IResourceController> controllers, Request request)
        {
            return
                from c in controllers
                where c.RequestType.IsInstanceOfType(request)
                select c;
        }
    }
}