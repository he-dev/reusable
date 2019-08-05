using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerProperty : LoggerMiddleware
    {
        private readonly IEnumerable<(string Name, object Value)> _properties;

        public LoggerProperty(IEnumerable<(string Name, object Value)> properties) : base(true)
        {
            _properties = properties;
        }

        public LoggerProperty(params (string Name, object Value)[] properties) : base(true)
        {
            _properties = properties;
        }

        protected override void InvokeCore(Abstractions.v2.Log request)
        {
            foreach (var (name, value) in _properties)
            {
                request[name] = value;
            }

            Next?.Invoke(request);
        }
    }
}