using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Newtonsoft.Json.Serialization;
using Reusable.Utilities.JsonNet.DependencyInjection;

namespace Reusable.Flexo.DependencyInjection
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterExpressions(this ContainerBuilder builder, IEnumerable<Type> customTypes = default)
        {
            foreach (var type in Expression.Types.Concat(customTypes ?? Enumerable.Empty<Type>()))
            {
                builder.RegisterType(type);
            }
        }
    }
}