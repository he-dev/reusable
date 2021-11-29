using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node creates properties using factories specified in <c>TryCreateProperties</c>.
/// </summary>
public class CreateProperty : LoggerNode
{
    private static AsyncScope<Stack<object>>? Scope => AsyncScope<Stack<object>>.Current;

    public override bool Enabled => AsyncScope<Stack<object>>.Any;

    public List<ITryCreateProperties> TryCreateProperties { get; set; } = new()
    {
        new TryCreatePropertiesFromEnum(),
        new TryCreatePropertiesFromException()
    };

    public static void Push(IEnumerable<object> items) => AsyncScope<Stack<object>>.Push(new Stack<object>(items));

    public override void Invoke(ILogEntry entry)
    {
        if (Scope is {} current)
        {
            foreach (var item in current.Value.Consume())
            {
                var nodes = item switch
                {
                    Action<ILogEntry> action => new[] { action },
                    IEnumerable<Action<ILogEntry>> actions => actions,
                    _ => default
                };

                if (nodes is {})
                {
                    foreach (var node in nodes)
                    {
                        node(entry);
                    }
                }
                else
                {
                    foreach (var property in CreateProperties(item))
                    {
                        entry.Push(property);
                    }
                }
            }

            current.Dispose();
        }

        InvokeNext(entry);
    }

    private IEnumerable<LogProperty> CreateProperties(object? obj)
    {
        if (obj is null)
        {
            yield break;
        }

        if (obj is LogProperty property)
        {
            yield return property;
            yield break;
        }

        if (obj is IEnumerable<LogProperty> properties)
        {
            foreach (var item in properties)
            {
                yield return item;
            }

            yield break;
        }

        foreach (var tryCreateProperties in TryCreateProperties)
        {
            var any = false;
            foreach (var p in tryCreateProperties.Invoke(obj))
            {
                yield return p;
                any = true;
            }

            if (any) yield break;
        }

        throw DynamicException.Create("UnsupportedObject", $"Could not create properties from '{obj.GetType().ToPrettyString()}'.");
    }
}

public static class PropertyFactoryNodeHelper
{
    public static ILogger PushProperties(this ILogger logger, IEnumerable<object>? items)
    {
        return logger.Also(_ => CreateProperty.Push(items ?? Enumerable.Empty<object>()));
    }
}

public interface ITryCreateProperties
{
    IEnumerable<LogProperty> Invoke(object obj);
}

public class TryCreatePropertiesFromEnum : ITryCreateProperties
{
    public IEnumerable<LogProperty> Invoke(object obj)
    {
        if (obj.GetType() is {IsEnum: true} type)
        {
            // Don't ToString the value because it will break the log-level.
            var name = type.GetCustomAttribute<PropertyNameAttribute>()?.ToString() ?? type.Name;
            yield return new LogProperty(name, obj, LogPropertyMeta.Builder.ProcessWith<Echo>());
        }
    }
}

public class TryCreatePropertiesFromException : ITryCreateProperties
{
    public IEnumerable<LogProperty> Invoke(object obj)
    {
        if (obj is Exception exception)
        {
            yield return new LogProperty(Names.Properties.Exception, exception, LogPropertyMeta.Builder.ProcessWith<Echo>());
        }
    }
}