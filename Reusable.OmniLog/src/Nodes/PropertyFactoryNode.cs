using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    [UsedImplicitly]
    public class PropertyFactoryNode : LoggerNode
    {
        private static AsyncScope<Stack<object>>? Scope => AsyncScope<Stack<object>>.Current;
        
        public override bool Enabled => AsyncScope<Stack<object>>.Any;

        public List<ITryCreateProperties> TryCreateProperties { get; set; } = new List<ITryCreateProperties>
        {
            new TryCreatePropertiesFromEnum(),
            new TryCreatePropertiesFromException()
        };

        public static void Push(IEnumerable<object> items) => AsyncScope<Stack<object>>.Push(new Stack<object>(items));

        public override void Invoke(ILogEntry request)
        {
            if (Scope is {} current)
            {
                foreach (var item in current.Value.Consume())
                {
                    foreach (var property in CreateProperties(item))
                    {
                        request.Add(property);
                    }
                }

                current.Dispose();
            }

            InvokeNext(request);
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

            if (obj is Exception)
            {
                yield return new LogProperty(Names.Default.Exception, obj, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
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
        public static void UsePropertyFactory(this ILogger logger, IEnumerable<object>? items)
        {
            PropertyFactoryNode.Push(items ?? Enumerable.Empty<object>());
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
                yield return new LogProperty(name, obj, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            }
        }
    }

    public class TryCreatePropertiesFromException : ITryCreateProperties
    {
        public IEnumerable<LogProperty> Invoke(object obj)
        {
            if (obj is Exception exception)
            {
                yield return new LogProperty(Names.Default.Exception, exception, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            }
        }
    }
}