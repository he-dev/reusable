using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    // when #Dump is Dictionary --> call Next() for each pair where Key: Identifier and Value: #Serializable
    // when #Dump is object --> call Next() for each property and its value where PropertyName: Identifier and Value: #Serializable
    // when #Dump is string --> call Next() once where Key.Name: Identifier and Value: #Dump as #Serializable
    /// <summary>
    /// Breaks a compound object into its component objects and create a log-entry for each one.
    /// </summary>
    public class OneToManyNode : LoggerNode
    {
        public OneToManyNode() : base(true) { }

        protected override void InvokeCore(LogEntry request)
        {
            var explodeCount = 0;

            foreach (var item in request.Where(x => x.Key.Tag.Equals(LogEntry.Tags.Explodable)).ToList())
            {
                var obj = item.Value;

                foreach (var (name, value) in obj.EnumerateProperties().Where(x => !(x.Value is null)))
                {
                    var copy = request.Clone();

                    copy.SetItem(LogEntry.Names.Object, LogEntry.Tags.Loggable, name);
                    copy.SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Serializable, value);

                    InvokeNext(copy);
                }
            }

            // There wasn't anything to explode so just invoke the next node. 
            if (explodeCount == 0)
            {
                Next?.Invoke(request);
            }

            void InvokeNext(LogEntry logEntry)
            {
                explodeCount++;
                Next?.Invoke(logEntry);
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
            return
                obj is IDictionary<string, object> dictionary
                    ? dictionary.Select(item => (item.Key, item.Value))
                    : obj
                        .GetType()
                        //.ValidateIsAnonymous()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(property => (property.Name, property.GetValue(obj)));
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