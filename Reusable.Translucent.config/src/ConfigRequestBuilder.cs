using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Reusable.Data;
using Reusable.Quickey;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ConfigRequestBuilder
    {
        public static Request CreateRequest
        (
            Reusable.Data.Option<RequestMethod> method,
            Selector selector,
            object? value = default,
            Action<ConfigRequest>? requestAction = default
        )
        {
            var resources =
                from m in selector.Member.Path()
                where m.IsDefined(typeof(SettingAttribute))
                select m.GetCustomAttribute<SettingAttribute>();

            var resource = resources.FirstOrDefault();

            var uri = UriStringHelper.CreateQuery
            (
                scheme: ConfigController.Scheme,
                path: new[] { "settings" },
                query: new (string Key, string Value)[] { (ConfigController.ResourceNameQueryKey, selector.ToString()) }
            );

            var request = new ConfigRequest
            {
                Uri = uri,
                Method = method,
                Body = value,
                SettingType = selector.DataType,
                ValidationAttributes = selector.Member.GetCustomAttributes<ValidationAttribute>(),
            };

            if (resource?.Controller is {} controller)
            {
                request.ControllerName = new ComplexName { controller };
            }

            requestAction?.Invoke(request);

            return request;
        }
    }
}