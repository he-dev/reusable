using System.Collections;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class ConstantNode : LoggerNode //, IEnumerable<(string Name, object Value)>
    {
        public ConstantNode() : base(true) { }

        public Dictionary<string, object> Constants { get; set; } = new Dictionary<string, object>();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var item in Constants)
            {
                request.SetItem(item.Key, default, item.Value);
            }

            Next?.Invoke(request);
        }

        //public void Add(string name, object value) => _properties.Add((name, value));

        //public IEnumerator<(string Name, object Value)> GetEnumerator() => _properties.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_properties).GetEnumerator();
    }
}