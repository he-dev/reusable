using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Reusable.Essentials.Extensions;
using Reusable.Octopus;
using Reusable.Octopus.Data;
using Reusable.ReMember;
using Reusable.Translucent.Annotations;

namespace Reusable.Translucent.Data;

public class ConfigRequest : Request
{
    public IEnumerable<ValidationAttribute> ValidationAttributes { get; set; } = Enumerable.Empty<ValidationAttribute>();

    public static ConfigRequest Create(Selector selector, RequestMethod method, object? body = default) => new()
    {
        ResourceName = { selector.ToString() },
        Method = method,
        Data = { body },
        ControllerFilter = selector.MemberType.GetCustomAttribute<ControllerNameAttribute>() is { } attr ? new ControllerFilterByName(attr) : default,
        ValidationAttributes = selector.MemberType.GetCustomAttributes<ValidationAttribute>()
    };
}

public class ConfigResponse : Response { }