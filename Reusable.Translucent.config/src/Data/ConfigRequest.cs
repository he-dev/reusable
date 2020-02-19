using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Reusable.Extensions;
using Reusable.Quickey;
using Reusable.Translucent.Annotations;

namespace Reusable.Translucent.Data
{
    public class ConfigRequest : Request
    {
        public Type SettingType { get; set; } = default!;

        public IEnumerable<ValidationAttribute> ValidationAttributes { get; set; } = Enumerable.Empty<ValidationAttribute>();

        public static Request Create
        (
            ResourceMethod method,
            Selector selector,
            object? value = default,
            Action<ConfigRequest>? configure = default
        )
        {
            var request = new ConfigRequest
            {
                ResourceName = selector.ToString(),
                Method = method,
                Body = value,
                SettingType = selector.DataType,
                ValidationAttributes = selector.Member.GetCustomAttributes<ValidationAttribute>(),
            };

            var attributes =
                from m in SelectorPath.Enumerate(selector.Member)
                where m.IsDefined(typeof(SettingAttribute))
                select m.GetCustomAttribute<SettingAttribute>();
            
            if (attributes.FirstOrDefault()?.Controller is {} controller)
            {
                request.ControllerName = controller;
            }
            
            request.Pipe(configure);

            return request;
        }
    }

    public class ConfigResponse : Response { }
}