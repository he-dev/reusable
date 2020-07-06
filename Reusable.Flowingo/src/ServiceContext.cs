using System;
using Autofac;
using Reusable.Extensions;

namespace Reusable.Flowingo
{
    public static class ServiceContext
    {
        public static IDisposable BeginScope(ILifetimeScope lifetimeScope)
        {
            return AsyncScope<ILifetimeScope>.Push(lifetimeScope);
        }

        public static T Resolve<T>(Action<T>? configure = default)
        {
            return
                AsyncScope<ILifetimeScope>.Current?.Value is {} lifetimeScope
                    ? lifetimeScope.Resolve<T>().Pipe(configure)
                    : throw new InvalidOperationException($"{nameof(ServiceContext)} is not initialized.");
        }
    }
}