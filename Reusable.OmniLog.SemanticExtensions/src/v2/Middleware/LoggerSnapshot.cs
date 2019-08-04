using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.SemanticExtensions.v2.Middleware
{
    public class LoggerSnapshot : LoggerMiddleware, IEnumerable<(Type Type, Func<object, object> Map)>
    {
        private readonly IDictionary<Type, Func<object, object>> _mappings = new Dictionary<Type, Func<object, object>>();

        public LoggerSnapshot() : base(true) { }

        public LoggerSnapshot Map<T>(Func<T, object> map)
        {
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
            if (request.TryGetValue(AbstractionProperties.Snapshot + ".Serializable", out var snapshot))
            {
                if (_mappings.TryGetValue(snapshot.GetType(), out var map))
                {
                    var obj = map(snapshot);
                    request["Snapshot.Serializable"] = obj;
                    Next?.Invoke(request);
                }
                else
                {
                    //var properties = snapshot.EnumerateProperties();
                    // todo - call Next for each property

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