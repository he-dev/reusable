using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Reusable.Utilities.AspNetCore.DependencyInjection
{
    public class AutofacLifetimeScopeBuilder
    {
        private readonly ContainerBuilder _builder;

        public AutofacLifetimeScopeBuilder(IServiceCollection services)
        {
            _builder = new ContainerBuilder();
            _builder.Populate(services);
        }

        public static AutofacLifetimeScopeBuilder From(IServiceCollection services)
        {
            return new AutofacLifetimeScopeBuilder(services);
        }

        public AutofacLifetimeScopeBuilder RegisterModule<T>() where T : IModule, new()
        {
            _builder.RegisterModule<T>();
            return this;
        }

        public AutofacLifetimeScopeBuilder Configure(Action<ContainerBuilder> configure)
        {
            configure(_builder);
            return this;
        }

        public ILifetimeScope Build() => _builder.Build();

        public IServiceProvider ToServiceProvider() => new AutofacServiceProvider(Build());
    }
}