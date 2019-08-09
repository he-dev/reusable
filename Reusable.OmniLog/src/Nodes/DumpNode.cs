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
    public class DumpNode : LoggerNode
    {
        public static class LogEntryItemTags
        {
            public static readonly string Object = nameof(Object);
        }

        public static class DefaultLogEntryItemNames
        {
            public static readonly string DumpName = nameof(DumpName);

            public static readonly string DumpValue = nameof(DumpValue);
        }

        public DumpNode() : base(true) { }

        public MappingCollection Mappings { get; set; } = new MappingCollection();

        public string DumpNameItem { get; set; } = DefaultLogEntryItemNames.DumpName;

        public string DumpValueItem { get; set; } = DefaultLogEntryItemNames.DumpValue;

        protected override void InvokeCore(LogEntry request)
        {
            var nextInvokeCount = 0;

            // Process only #Object items.
            foreach (var item in request.Where(x => x.Key.Tag.Equals(LogEntryItemTags.Object)).ToList())
            {
                var dump = item.Value;

                switch (dump)
                {
                    case string str:
                        // There is nothing more to do. Just save it as a normal property.
                        request.SetItem(DumpNameItem, default, item.Key.Name);
                        request.SetItem(DumpValueItem, default, str);
                        InvokeNext(request);
                        break;

                    default:
                        // Do we have a custom mapping for the dump?
                        if (Mappings.TryGetMapping(dump.GetType(), out var map))
                        {
                            dump = map(dump);
                            request.SetItem(DumpNameItem, default, item.Key.Name);
                            request.SetItem(SerializationNode.CreateRequestItemKey(DumpValueItem), dump);
                            InvokeNext(request);
                        }
                        // No? Then enumerate all its properties.
                        else
                        {
                            foreach (var (name, value) in dump.EnumerateProperties().Where(x => !(x.Value is null)))
                            {
                                var copy = request.Clone();
                                
                                if (Mappings.TryGetMapping(value.GetType(), out map))
                                {
                                    dump = map(value);
                                    copy.SetItem(DumpNameItem, default, item.Key.Name);
                                    copy.SetItem(SerializationNode.CreateRequestItemKey(DumpValueItem), dump);
                                }
                                else
                                {
                                    copy.SetItem(DumpNameItem, default, name);
                                    copy.SetItem(SerializationNode.CreateRequestItemKey(DumpValueItem), value);
                                }

                                InvokeNext(copy);
                            }
                        }

                        break;
                }
            }
            
            // There wasn't anything to dump.
            if (nextInvokeCount == 0)
            {
                InvokeNext(request);
            }

            void InvokeNext(LogEntry logEntry)
            {
                nextInvokeCount++;
                Next?.Invoke(logEntry);
            }
        }

        public static ItemKey<SoftString> CreateRequestItemKey(SoftString name) => new ItemKey<SoftString>(name, LogEntryItemTags.Object);

        public static class Mapping
        {
            public static (Type Type, Func<object, object> Map) For<T>(Func<T, object> map)
            {
                // We can store only Func<object, object> but the call should be able
                // to use T so we need to cast the parameter from 'object' to T.

                // Compile: map((T)obj)
                var parameter = Expression.Parameter(typeof(object), DefaultLogEntryItemNames.DumpValue);
                var mapFunc =
                    Expression.Lambda<Func<object, object>>(
                            Expression.Call(
                                Expression.Constant(map.Target),
                                map.Method,
                                Expression.Convert(parameter, typeof(T))),
                            parameter)
                        .Compile();

                return (typeof(T), mapFunc);
            }
        }

        public class MappingCollection : IEnumerable<KeyValuePair<Type, Func<object, object>>>
        {
            private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

            public void Add((Type Type, Func<object, object> Map) mapping) => _mappings.Add(mapping.Type, mapping.Map);

            public bool TryGetMapping(Type type, out Func<object, object> map) => _mappings.TryGetValue(type, out map);

            public IEnumerator<KeyValuePair<Type, Func<object, object>>> GetEnumerator() => _mappings.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    public static class DumpNodeHelper
    {
        public static LogEntry Dump(this LogEntry logEntry, object obj)
        {
            return logEntry.SetItem(nameof(Dump), DumpNode.LogEntryItemTags.Object, obj);
        }
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