using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class EchoNode : LoggerNode
    {
        public override bool Enabled => Rx?.Any() == true;

        public List<ILogRx> Rx { get; set; } = new List<ILogRx>();

        protected override void invoke(ILogEntry request)
        {
            var echo = new LogEntryEcho(request);

            foreach (var rx in Rx)
            {
                rx.Log(echo);
            }

            Next?.Invoke(request);
        }

        private class LogEntryEcho : ILogEntry
        {
            private readonly ILogEntry _entry;

            public LogEntryEcho(ILogEntry entry) => _entry = entry;

            public LogProperty? this[SoftString name] => TryGetProperty(name, out var property) ? property : default;

            public void Add(LogProperty property) => _entry.Add(property);

            public bool TryGetProperty(SoftString name, out LogProperty property)
            {
                return _entry.TryGetProperty(name, out property) && LogProperty.CanProcess.With<EchoNode>()(property);
            }

            public ILogEntry Copy() => _entry.Copy();

            public IEnumerator<LogProperty> GetEnumerator() => _entry.Where(LogProperty.CanProcess.With<EchoNode>()).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entry).GetEnumerator();
        }
    }
}