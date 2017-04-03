using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reusable.Colin
{
    internal static class ParameterReflector
    {
        public static IEnumerable<Data.ParameterInfo> GetParameters(Type parameterType)
        {
            return
                parameterType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                    .Select(p => new Data.ParameterInfo(p));
        }
    }
}