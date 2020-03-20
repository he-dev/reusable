using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class DelegateNode : LoggerNode
    {
        public override bool Enabled => AsyncScope<Item>.Any;

        public static void Push(Item item) => AsyncScope<Item>.Push(item);

        public override void Invoke(ILogEntry request)
        {
            while (Enabled)
            {
                using var item = AsyncScope<Item>.Current!.Value!;
                item.AlterLogEntry(request);
            }

            InvokeNext(request);
        }

        public class Item : IDisposable
        {
            public Item(AlterLogEntryDelegate alterLogEntry)
            {
                AlterLogEntry = alterLogEntry;
            }

            public AlterLogEntryDelegate AlterLogEntry { get; }

            public void Dispose() => AsyncScope<Item>.Current?.Dispose();
        }
    }

    public static class LoggerLambdaHelper
    {
        public static void UseDelegate(this ILogger logger, AlterLogEntryDelegate alter)
        {
            DelegateNode.Push(new DelegateNode.Item(alter));
        }
    }

    public class StackNode : LoggerNode
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

        private IEnumerable<LogProperty> CreateProperties(object obj)
        {
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
            }
            else
            {
                yield return new LogProperty(LogProperty.Names.Snapshot, obj, LogPropertyMeta.Builder.ProcessWith<DestructureNode>());
            }
        }
    }

    public static class StackNodeHelper
    {
        public static void UseStack(this ILogger logger, IEnumerable<object> items)
        {
            StackNode.Push(items);
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
    
    public class WorkItemNode : LoggerNode
    {
        public string PropertyName { get; set; } = "Category";

        public Exception? Exception { get; set; }
        
        public override void Invoke(ILogEntry request)
        {
            if (request.TryGetProperty("Category", out var property) && SoftString.Comparer.Equals(property.Value?.ToString(), "WorkItem"))
            {
                var value = request[LogProperty.Names.Snapshot].Value.Value;
                ((IDictionary<string, object>)value).Add("status", WorkItemStatus.Begin);
                var logger = ((ILoggerNode)this).First().Node<Logger>();
                var scope = ((ILoggerNode)this).First().Node<Logger>().Scope();
                scope.Append(new WorkItemEnd { Logger = logger });
            }
            InvokeNext(request);
        }

        private class WorkItemEnd : LoggerNode
        {
            public ILogger Logger { get; set; }
            
            private Action Log { get; set; }
            
            public override void Invoke(ILogEntry request)
            {
                Log = () => Logger.Log(Snapshot.Take("workItemTest", new { status = WorkItemStatus.Completed }));
                InvokeNext(request);
            }

            public override void Dispose()
            {
                Log();
                base.Dispose();
            }
        }
    }

    public static class WorkItemHelper
    {
        public static WorkItemNode WorkItem(this ScopeNode logger) => logger.Node<WorkItemNode>();
    }

    public enum WorkItemStatus
    {
        Begin,
        Completed,
        Canceled,
        Faulted
    }
}