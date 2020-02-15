using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IResourceController> ResourceControllerFilterDelegate(IEnumerable<IResourceController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByControllerName(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.ControllerName.Equals(ControllerName.Any))
            {
                if (request.ControllerName.Tags.Any())
                {
                    return
                        from c in controllers
                        where c.Name.Tags.Overlaps(request.ControllerName.Tags)
                        select c;
                }
                else
                {
                    return controllers;
                }
            }
            else
            {
                return
                    from c in controllers
                    where c.Name.Equals(request.ControllerName)
                    select c;
            }
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