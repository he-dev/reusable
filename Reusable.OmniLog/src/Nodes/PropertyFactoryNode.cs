using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class PropertyFactoryNode : LoggerNode
    {
        public override bool Enabled => AsyncScope<Stack<object>>.Any;

        public Dictionary<Type, ICreateProperty> PropertyFactories { get; set; } = new Dictionary<Type, ICreateProperty>
        {
            [typeof(Enum)] = new CreatePropertyFromEnum()
        };

        public static void Push(IEnumerable<object> items) => AsyncScope<Stack<object>>.Push(new Stack<object>(items));

        public override void Invoke(ILogEntry request)
        {
            if (Enabled)
            {
                foreach (var item in AsyncScope<Stack<object>>.Current.Value.Consume())
                {
                    foreach (var property in CreateProperties(item))
                    {
                        request.Add(property);
                    }
                }
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
                yield return new LogProperty(LogProperty.Names.Exception, obj, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
                yield break;
            }

            var type = obj.GetType() switch
            {
                {IsEnum: true} => typeof(Enum),
                {} t => t
            };

            if (PropertyFactories.TryGetValue(type, out var createProperty))
            {
                foreach (var item in createProperty.Invoke(obj))
                {
                    yield return item;
                }

                yield break;
            }

            yield return new LogProperty(LogProperty.Names.Snapshot, obj, LogPropertyMeta.Builder.ProcessWith<DestructureNode>());
        }
    }
    
    public static class StackNodeHelper
    {
        public static void UseStack(this ILogger logger, IEnumerable<object> items)
        {
            PropertyFactoryNode.Push(items);
        }
    }

    public interface ICreateProperty
    {
        IEnumerable<LogProperty> Invoke(object obj);
    }

    public class CreatePropertyFromEnum : ICreateProperty
    {
        public IEnumerable<LogProperty> Invoke(object obj)
        {
            yield return new LogProperty(obj.GetType().Name, obj.ToString(), LogPropertyMeta.Builder.ProcessWith<EchoNode>());
        }
    }

    public static class Snapshot
    {
        public static IEnumerable<LogProperty> Take(string name, object dump)
        {
            yield return new LogProperty(LogProperty.Names.SnapshotName, name, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(LogProperty.Names.Snapshot, dump.ToDictionary(), LogPropertyMeta.Builder.ProcessWith<SerializerNode>());
        }
    }

    public static class CallerInfo
    {
        public static IEnumerable<LogProperty> Create
        (
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            yield return new LogProperty(LogProperty.Names.CallerMemberName, callerMemberName, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(LogProperty.Names.CallerLineNumber, callerLineNumber, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(LogProperty.Names.CallerFilePath, callerFilePath, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
        }
    }
}