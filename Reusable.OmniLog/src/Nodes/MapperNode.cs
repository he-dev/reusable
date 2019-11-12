using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public class MapperNode : LoggerNode
    {
        public MappingCollection Mappings { get; set; } = new MappingCollection();

        protected override void invoke(LogEntry request)
        {
            foreach (var item in request.Where(x => x.Key.Tag.Equals(LogEntry.Tags.Serializable)).ToList())
            {
                var obj = item.Value;

                // Do we have a custom mapping for the dump?
                if (Mappings.TryGetMapping(obj.GetType(), out var map))
                {
                    obj = map(obj);
                    request.SetItem(item.Key, obj); // Replace the original object.
                }
            }

            Next?.Invoke(request);
        }

        public class Mapping
        {
            public Type Type { get; private set; }

            public Func<object, object> Map { get; private set; }

            public static Mapping For<T>(Func<T, object> map)
            {
                // We can store only Func<object, object> but the call should be able
                // to use T so we need to cast the parameter from 'object' to T.

                // Compile: map((T)obj)
                var parameter = Expression.Parameter(typeof(object), LogEntry.Tags.Serializable);
                var mapFunc =
                    Expression.Lambda<Func<object, object>>(
                            Expression.Call(
                                Expression.Constant(map.Target),
                                map.Method,
                                Expression.Convert(parameter, typeof(T))),
                            parameter)
                        .Compile();

                return new Mapping { Type = typeof(T), Map = mapFunc };
            }
        }

        public class MappingCollection : IEnumerable<KeyValuePair<Type, Func<object, object>>>
        {
            private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

            public void Add(Mapping mapping) => _mappings.Add(mapping.Type, mapping.Map);

            public void AddRange(IEnumerable<Mapping> mappings)
            {
                foreach (var mapping in mappings)
                {
                    Add(mapping);
                }
            }

            public bool TryGetMapping(Type type, out Func<object, object> map) => _mappings.TryGetValue(type, out map);

            public IEnumerator<KeyValuePair<Type, Func<object, object>>> GetEnumerator() => _mappings.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    public static class MapperNodeHelper
    {
//        public static LogEntry Dump(this LogEntry logEntry, object obj)
//        {
//            return logEntry.SetItem(nameof(Dump), OneToManyNode.LogEntryItemTags.Explodable, obj);
//        }
    }
}