using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node maps one object into a different one.
/// </summary>
public class MapSnapshot : LoggerNode
{
    public MappingCollection Mappings { get; set; } = new();

    public override void Invoke(ILogEntry entry)
    {
        if (entry.TryGetProperty<SerializableProperty.Snapshot>(out var snapshot))
        {
            // Do we have a custom mapping for the dump?
            if (Mappings.TryGetMapping(snapshot.Value.GetType(), out var map))
            {
                var obj = map(snapshot.Value);
                entry.Push(new SerializableProperty.Snapshot(obj)); // Replace the original object.
            }
        }
        
        InvokeNext(entry);
    }

    public class Mapping
    {
        private Mapping(Type type, Func<object, object> map)
        {
            Type = type;
            Map = map;
        }

        public Type Type { get; }

        public Func<object, object> Map { get; }

        public static Mapping For<T>(Func<T, object> map)
        {
            // We can store only Func<object, object> but the call should be able
            // to use T so we need to cast the parameter from 'object' to T.

            // Compile: map((T)obj)
            var parameter = Expression.Parameter(typeof(object), "obj");
            var mapFunc =
                Expression.Lambda<Func<object, object>>(
                        Expression.Call(
                            Expression.Constant(map.Target),
                            map.Method,
                            Expression.Convert(parameter, typeof(T))),
                        parameter)
                    .Compile();

            return new Mapping(typeof(T), mapFunc);
        }
    }

    public class MappingCollection : IEnumerable<KeyValuePair<Type, Func<object, object>>>
    {
        private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

        public void Add(Mapping mapping) => _mappings.Add(mapping.Type, mapping.Map);

        public void Add<T>(Func<T, object> map) => Add(Mapping.For(map));

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