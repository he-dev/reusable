using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Middleware;

namespace Reusable.OmniLog.SemanticExtensions.Middleware
{
    public class LoggerDump : LoggerMiddleware, IEnumerable<(Type Type, Func<object, object> Map)>
    {
        public static readonly string LogItemTag = "Dump";

        private const string Identifier = nameof(Identifier);
        private const string Dump = nameof(Dump);

        private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

        public LoggerDump() : base(true) { }

        public IDictionary<string, string> LogPropertyNames { get; set; } = new Dictionary<string, string>
        {
            ["Identifier"] = "Identifier",
            ["Dump"] = "Snapshot"
        };
        
        public void Add((Type Type, Func<object, object> Map) mapping) => _mappings.Add(mapping.Type, mapping.Map);

        protected override void InvokeCore(Log request)
        {
            // todo - use key-name as identifier when snapshot is a string, e.g. decision
            
            // Do we have a snapshot?
            if (request.TryGetItem<object>((nameof(LoggerSnapshotHelper.Snapshot), LogItemTag), out var snapshot))
            {
                // Do we have a custom mapping for the snapshot?
                if (_mappings.TryGetValue(snapshot.GetType(), out var map))
                {
                    var obj = map(snapshot);
                    request.Serializable(LogPropertyNames[Dump], obj);
                    Next?.Invoke(request);
                }
                // No? Then enumerate all its properties.
                else
                {
                    var propertiesEnumerated = false;
                    foreach (var (name, value) in snapshot.EnumerateProperties())
                    {
                        var copy = request.Clone();

                        copy.SetItem((LogPropertyNames[Identifier], default), name);
                        copy.Serializable(LogPropertyNames[Dump], value);

                        Next?.Invoke(copy);

                        propertiesEnumerated = true;
                    }

                    // Call 'Next' as usual when the snapshot was empty.
                    if (!propertiesEnumerated)
                    {
                        Next?.Invoke(request);
                    }
                }
            }
            // No snapshot? Just do the usual.
            else
            {
                Next?.Invoke(request);
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
                var parameter = Expression.Parameter(typeof(object), nameof(LoggerSnapshotHelper.Snapshot));
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
    }

    public static class LoggerSnapshotHelper
    {
        public static Log Snapshot(this Log log, object obj)
        {
            return log.SetItem((nameof(Snapshot), LoggerDump.LogItemTag), obj);
        }
    }
}