using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Utilities.JsonNet.TypeDictionaries;

/// <summary>
/// Gets all types in the current domain that are decorated with the <see cref="JsonTypeSchemaAttribute"/>.
/// </summary>
public class CurrentDomainTypeDictionary : CustomTypeDictionary
{
    public CurrentDomainTypeDictionary() : base(CurrentDomainTypes())
    {
            
    }

    private static IEnumerable<Type> CurrentDomainTypes()
    {
        return
            from a in AppDomain.CurrentDomain.GetAssemblies()
            from t in a.GetTypes()
            where t.IsClass && t.IsDefined(typeof(JsonTypeSchemaAttribute), inherit: true)
            select t;
    }
}