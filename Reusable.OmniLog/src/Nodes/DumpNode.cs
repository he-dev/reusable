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
    public class DumpNode : LoggerNode, IEnumerable<(Type Type, Func<object, object> Map)>
    {
        public static readonly string LogItemTag = "Object";

        private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

        public DumpNode() : base(true) { }

        public Property Variable { get; set; } = new Property.Variable();
        
        public Property Dump { get; set; } = new Property.Dump();

        public void Add((Type Type, Func<object, object> Map) mapping) => _mappings.Add(mapping.Type, mapping.Map);

        protected override void InvokeCore(LogEntry request)
        {
            var nextInvokeCount = 0;

            // Process only #Object items.
            foreach (var item in request.Where(x => x.Key.Tag.Equals(LogItemTag)).ToList())
            {
                var dump = item.Value;

                switch (dump)
                {
                    case string str:
                        // There is nothing more to do. Just save it as a normal property.
                        request.SetItem(Variable, default, item.Key.Name);
                        request.SetItem(Dump, default, str);
                        InvokeNext(request);
                        break;

                    default:
                        // Do we have a custom mapping for the dump?
                        if (_mappings.TryGetValue(dump.GetType(), out var map))
                        {
                            dump = map(dump);
                            request.SetItem(Variable, default, item.Key.Name);
                            request.Serializable(Dump, dump);
                            InvokeNext(request);
                        }
                        // No? Then enumerate all its properties.
                        else
                        {
                            foreach (var (name, value) in dump.EnumerateProperties())
                            {
                                // todo - not dry
                                if (!(value is null) && _mappings.TryGetValue(value.GetType(), out map))
                                {
                                    dump = map(value);
                                    request.SetItem(Variable, default, item.Key.Name);
                                    request.Serializable(Dump, dump);
                                    InvokeNext(request);
                                }
                                else
                                {
                                    var copy = request.Clone();
                                    copy.SetItem(Variable, default, name);
                                    copy.Serializable(Dump, value);
                                    InvokeNext(copy);
                                }
                            }
                        }
                        break;
                }
            }

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

        public IEnumerator<(Type Type, Func<object, object> Map)> GetEnumerator() => _mappings.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static class Mapping
        {
            public static (Type Type, Func<object, object> Map) Map<T>(Func<T, object> map)
            {
                // We can store only Func<object, object> but the call should be able
                // to use T so we need to cast the parameter from 'object' to T.

                // Compile: map((T)obj)
                var parameter = Expression.Parameter(typeof(object), nameof(Property.Dump));
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

        public abstract class Property
        {
            public string Name { get; }
        
            protected Property(string name) => Name = name;
        
            public static implicit operator string(Property property) => property.Name;
        
            public static implicit operator SoftString(Property property) => property.Name;
        
            public class Variable : Property
            {
                public Variable(string name = default) : base(name ?? nameof(Variable)) { }
            }
        
            public class Dump : Property
            {
                public Dump(string name = default) : base(name ?? nameof(Dump)) { }
            }
        }
    }

    public static class LoggerDumpHelper
    {
        public static LogEntry Dump(this LogEntry logEntry, object obj)
        {
            return logEntry.SetItem(nameof(Dump), DumpNode.LogItemTag, obj);
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