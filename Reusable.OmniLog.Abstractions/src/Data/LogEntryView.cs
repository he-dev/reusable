using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog {
    public class LogEntryView<T> : ILogEntry where T : ILoggerNode
    {
        private readonly ILogEntry _entry;

        public LogEntryView(ILogEntry entry) => _entry = entry;

        public LogProperty? this[SoftString name] => TryGetProperty(name, out var property) ? property : default;

        public void Add(LogProperty property) => _entry.Add(property);

        public bool TryGetProperty(SoftString name, out LogProperty property)
        {
            return _entry.TryGetProperty(name, out property) && LogProperty.CanProcess.With<T>()(property);
        }

        public ILogEntry Copy() => _entry.Copy();

        public IEnumerator<LogProperty> GetEnumerator() => _entry.Where(LogProperty.CanProcess.With<T>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entry).GetEnumerator();
    }
}