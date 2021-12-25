using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Reusable.Essentials.Extensions;
using Reusable.Octopus;
using Reusable.Octopus.Data;
using Reusable.Quickey;
using Reusable.Translucent.Annotations;

namespace Reusable.Translucent.Data;

public abstract class ConfigRequest : Request
{
    public IEnumerable<ValidationAttribute> ValidationAttributes { get; set; } = Enumerable.Empty<ValidationAttribute>();

    public class Json : ConfigRequest, IJsonRequest
    {
        public Type BodyType { get; private init; } = null!;

        public static Json Read(Selector selector) => new()
        {
            ResourceName = { selector.ToString() },
            Method = RequestMethod.Read,
            BodyType = selector.MemberType,
            ControllerFilter = selector.MemberType.GetCustomAttribute<ControllerNameAttribute>() is {} attr ? new ControllerFilterByName(attr) : default
        };
        
        public static Json Write(Selector selector, object data) => new()
        {
            ResourceName = { selector.ToString() },
            Method = RequestMethod.Read,
            BodyType = selector.MemberType,
            Body = { data },
            ControllerFilter = selector.MemberType.GetCustomAttribute<ControllerNameAttribute>() is {} attr ? new ControllerFilterByName(attr) : default
        };
    }
    
    public class Text : ConfigRequest
    {
        public static Text Read(Selector selector) => new()
        {
            ResourceName = { selector.ToString() },
            Method = RequestMethod.Read,
            ControllerFilter = selector.MemberType.GetCustomAttribute<ControllerNameAttribute>() is {} attr ? new ControllerFilterByName(attr) : default
        };
        
        public static Text Write(Selector selector, object data) => new()
        {
            ResourceName = { selector.ToString() },
            Method = RequestMethod.Read,
            Body = { data },
            ControllerFilter = selector.MemberType.GetCustomAttribute<ControllerNameAttribute>() is {} attr ? new ControllerFilterByName(attr) : default
        };
    }
}

public class ConfigResponse : Response { }