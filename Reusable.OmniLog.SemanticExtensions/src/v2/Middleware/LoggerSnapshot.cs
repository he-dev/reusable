using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.v2;
using Reusable.OmniLog.v2.Middleware;

namespace Reusable.OmniLog.SemanticExtensions.v2.Middleware
{
    using acpn = AbstractionContext.PropertyNames;
    
    public class LoggerSnapshot : LoggerMiddleware, IEnumerable<(Type Type, Func<object, object> Map)>
    {
        private static readonly string SnapshotKey = LoggerSerializer.CreateDataKey(AbstractionProperties.Snapshot);

        private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

        public LoggerSnapshot() : base(true) { }

        public LoggerSnapshot Map<T>(Func<T, object> map)
        {
            // We can store only Func<object, object> but the call should be able
            // to use T so we need to cast the parameter from 'object' to T.

            // Compile: map((T)obj)
            var parameter = Expression.Parameter(typeof(object), "Snapshot");
            var mapFunc =
                Expression.Lambda<Func<object, object>>(
                        Expression.Call(
                            Expression.Constant(map.Target),
                            map.Method,
                            Expression.Convert(parameter, typeof(T))),
                        parameter)
                    .Compile();
            _mappings.Add(typeof(T), mapFunc);

            return this;
        }

        protected override void InvokeCore(Abstractions.v2.Log request)
        {
            // Do we have a snapshot?
            if (request.TryGetValue(SnapshotKey, out var snapshot))
            {
                // Do we have a custom mapping for the snapshot?
                if (_mappings.TryGetValue(snapshot.GetType(), out var map))
                {
                    var obj = map(snapshot);
                    request.AttachSerializable(acpn.Snapshot, obj);
                    Next?.Invoke(request);
                }
                // No? Then enumerate all its properties.
                else
                {
                    var propertiesEnumerated = false;
                    foreach (var (name, value) in snapshot.EnumerateProperties())
                    {
                        var copy = new Log(request);

                        copy.SetItem(acpn.Identifier, name);
                        copy.AttachSerializable(acpn.Snapshot, value);

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
    }
}