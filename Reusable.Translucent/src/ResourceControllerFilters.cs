using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IController> ResourceControllerFilterDelegate(IEnumerable<IController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IController> FilterByControllerName(this IEnumerable<IController> controllers, Request request)
        {
            if (request.ControllerName.Equals(ControllerName.Empty))
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

        public static IEnumerable<IController> FilterByRequest(this IEnumerable<IController> controllers, Request request)
        {
            return
                from c in controllers
                where c.GetType().GetCustomAttribute<HandlesAttribute>().Type.IsInstanceOfType(request)
                select c;
        }

        // public static IEnumerable<IResourceController> FilterByUriPath(this IEnumerable<IResourceController> controllers, Request request)
        // {
        //     if (request.ResourceName.IsAbsolute)
        //     {
        //         return controllers;
        //     }
        //
        //     return
        //         from c in controllers
        //         where c.BaseUri is {}
        //         select c;
        // }
    }
}