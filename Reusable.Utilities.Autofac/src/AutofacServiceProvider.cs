using System;
using Autofac;

namespace Reusable.Utilities.Autofac
{
    public interface IServiceProviderEx : IServiceProvider
    {
        bool IsRegistered(Type type);
    }

    public class AutofacServiceProvider : IServiceProviderEx
    {
        private readonly IComponentContext _componentContext;

        public AutofacServiceProvider(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public bool IsRegistered(Type type) => _componentContext.IsRegistered(type);

        public object GetService(Type type) => _componentContext.Resolve(type);
    }
}