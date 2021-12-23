using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Reusable.Utilities.JsonNet.Serialization;

public class InterfaceContractResolver<T> : DefaultContractResolver
{
    public InterfaceContractResolver()
    {
        if (!typeof(T).IsInterface) throw new InvalidOperationException("T must be an interface.");
    }

    public override JsonContract ResolveContract(Type type)
    {
        return 
            typeof(T).IsAssignableFrom(type) 
                ? base.ResolveContract(typeof(T)) 
                : default!;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return
            typeof(T).IsAssignableFrom(type)
                ? base.CreateProperties(typeof(T), memberSerialization)
                : default!;
    }
}