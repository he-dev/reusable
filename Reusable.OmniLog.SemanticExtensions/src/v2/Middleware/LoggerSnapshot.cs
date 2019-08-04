using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.SemanticExtensions.v2.Middleware
{
    public class LoggerSnapshot : LoggerMiddleware, IEnumerable<(Type Type, Func<Type, object> Map)>
    {
        private readonly IDictionary<Type, Func<Type, object>> _mappings = new Dictionary<Type, Func<Type, object>>();
        
        public LoggerSnapshot() : base(true) { }

        public void Add<T>(Func<T, object> map)
        {
            
        }

        protected override void InvokeCore(ILog request)
        {
            if (request.TryGetValue(AbstractionProperties.Snapshot, out var snapshot))
            {
                var properties = snapshot.EnumerateProperties();
            }
            else
            {
                Next?.Invoke(request);
            }
        }

        public IEnumerator<(Type Type, Func<Type, object> Map)> GetEnumerator() => _mappings.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}