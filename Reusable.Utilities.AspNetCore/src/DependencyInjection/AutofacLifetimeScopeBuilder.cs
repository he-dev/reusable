using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Marbles;

namespace Reusable.Utilities.AspNetCore.DependencyInjection
{
    [PublicAPI]
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
            return new(services);
        }

        public AutofacLifetimeScopeBuilder Configure(Action<ContainerBuilder> configure)
        {
            return this.Also(_ => configure(_builder));
        }

        public ILifetimeScope Build() => _builder.Build();

        public IServiceProvider ToServiceProvider() => new AutofacServiceProvider(Build());
    }
}