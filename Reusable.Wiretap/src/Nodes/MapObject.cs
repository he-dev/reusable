using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node maps one object into a different one.
    /// </summary>
    public class MapObject : LoggerNode
    {
        public MappingCollection Mappings { get; set; } = new MappingCollection();

        public override void Invoke(ILogEntry request)
        {
            foreach (var property in request.Where(LogProperty.CanProcess.With<SerializeProperty>()).ToList())
            {
                // Do we have a custom mapping for the dump?
                if (property.Value is {} && Mappings.TryGetMapping(property.Value.GetType(), out var map))
                {
                    var obj = map(property.Value);
                    request.Push(property.Name, obj, LogProperty.Process.With<SerializeProperty>()); // Replace the original object.
                }
            }

            InvokeNext(request);
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
}