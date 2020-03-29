using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Reflection
{
    public static class TypeHelper
    {
        public static IEnumerable<Type> GetTypesAssignableFrom<T>()
        {
            return
                from t in typeof(T).Assembly.GetTypes()
                where t.IsClass && typeof(T).IsAssignableFrom(t)
                select t;
        }
    }
}