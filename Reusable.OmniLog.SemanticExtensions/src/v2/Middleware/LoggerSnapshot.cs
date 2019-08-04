using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;
using Reusable.OmniLog.v2;
using Reusable.OmniLog.v2.Middleware;

namespace Reusable.OmniLog.SemanticExtensions.v2.Middleware
{
    public class LoggerSnapshot : LoggerMiddleware, IEnumerable<(Type Type, Func<object, object> Map)>
    {
        private static readonly string DataKey = LoggerSerializer.CreateDataKey(AbstractionProperties.Snapshot);

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

        protected override void InvokeCore(ILog request)
        {
            if (request.TryGetValue(DataKey, out var snapshot))
            {
                if (_mappings.TryGetValue(snapshot.GetType(), out var map))
                {
                    var obj = map(snapshot);
                    request[DataKey] = obj;
                    Next?.Invoke(request);
                }
                else
                {
                    var empty = true;
                    foreach (var (name, value) in snapshot.EnumerateProperties())
                    {
                        var copy = new Log(request);

                        copy.SetItem(AbstractionProperties.Identifier, name);
                        copy.AttachSerializable(AbstractionProperties.Snapshot, value);

                        Next?.Invoke(copy);

                        empty = false;
                    }

                    // Call next as usual when the snapshot was empty.
                    if (empty)
                    {
                        Next?.Invoke(request);
                    }
                }
            }
            else
            {
                Next?.Invoke(request);
            }
        }

        public IEnumerator<(Type Type, Func<object, object> Map)> GetEnumerator() => _mappings.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}