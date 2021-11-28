using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Utilities.Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterEnumerable<T>
        (
            this ContainerBuilder builder,
            IEnumerable<T> source,
            Action<IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>>? configure = default
        ) where T : class
        {
            foreach (var item in source)
            {
                builder.RegisterInstance(item).Also(configure);
            }
        }

        public static void RegisterWith<T>
        (
            this IEnumerable<T> source,
            ContainerBuilder builder,
            Action<IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>>? configure = default
        ) where T : class
        {
            foreach (var item in source)
            {
                builder.RegisterInstance(item).Also(configure);
            }
        }
    }
}