using System;

namespace Reusable.DoubleDash.Annotations;

public class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type? serviceType = default) => ServiceType = serviceType;

    public Type? ServiceType { get; }
}