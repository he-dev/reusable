using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    // when #Dump is Dictionary --> call Next() for each pair where Key: Identifier and Value: #Serializable
    // when #Dump is object --> call Next() for each property and its value where PropertyName: Identifier and Value: #Serializable
    // when #Dump is string --> call Next() once where Key.Name: Identifier and Value: #Dump as #Serializable
    /// <summary>
    /// Breaks a compound object into its component objects and create a log-entry for each one.
    /// </summary>
    public class DestructureNode : LoggerNode
    {
        public override void Invoke(ILogEntry request)
        {
            var explodable =
                from p in request.Where(LogProperty.CanProcess.With(this))
                from x in p.Value.EnumerateProperties().Where(x => x.Value is {})
                select x;

            if (explodable.ToList() is var items && items.Any())
            {
                foreach (var (name, value) in items)
                {
                    var copy = request.Copy();

                    copy.Add(LogProperty.Names.SnapshotName, name, m => m.ProcessWith<EchoNode>());
                    copy.Add(LogProperty.Names.Snapshot, value, m => m.ProcessWith<SerializerNode>());

                    InvokeNext(copy);
                }
            }
            // There wasn't anything to explode so just invoke the next node. 
            else
            {
                InvokeNext(request);
            }
        }
    }

    public static class OneToManyHelper
    {
//        public static LogEntry Dump(this LogEntry logEntry, object obj)
//        {
//            return logEntry.SetItem(nameof(Dump), OneToManyNode.LogEntryItemTags.Explodable, obj);
//        }
    }

    internal static class ObjectExtensions
    {
        public static IEnumerable<(string Name, object Value)> EnumerateProperties<T>(this T obj)
        {
            return obj switch
            {
                IDictionary<string, object> d => d.Select(item => (item.Key, item.Value)),
                {} => obj
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => (property.Name, property.GetValue(obj))),
                _ => Enumerable.Empty<(string, object)>()
            };
        }

        private static Type ValidateIsAnonymous(this Type type)
        {
            var isAnonymous = type.Name.StartsWith("<>f__AnonymousType");

            return
                isAnonymous
                    ? type
                    : throw DynamicException.Create("Snapshot", "Snapshot must be either an anonymous type or a dictionary");
        }
    }
}