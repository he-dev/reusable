using System;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace Reusable.Utilities.Autofac;

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

[AttributeUsage(AttributeTargets.Property)]
public class ServiceAttribute : Attribute { }

public class AttributeSelector<T> : IPropertySelector where T : Attribute
{
    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
    {
        return propertyInfo.GetCustomAttribute<T>() is { };
    }
}