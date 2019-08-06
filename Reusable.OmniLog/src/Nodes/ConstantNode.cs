using System.Collections;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class ConstantNode : LoggerNode, IEnumerable<(string Name, object Value)>
    {
        private readonly IList<(string Name, object Value)> _properties;

        public ConstantNode() : base(true)
        {
            _properties = new List<(string Name, object Value)>();
        }

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var (name, value) in _properties)
            {
                request.SetItem(name, default,  value);
            }

            Next?.Invoke(request);
        }

        public void Add(string name, object value) => _properties.Add((name, value));

        public IEnumerator<(string Name, object Value)> GetEnumerator() => _properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_properties).GetEnumerator();
    }
}