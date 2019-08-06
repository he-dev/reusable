using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
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

        protected override void InvokeCore(Log request)
        {
            foreach (var (name, value) in _properties)
            {
                request.SetItem(name, default,  value);
            }

            Next?.Invoke(request);
        }
    }
}