using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class LogEntryView<T> : ILogEntry where T : class, ILoggerNode
    {
        private readonly ILogEntry _entry;

        public LogEntryView(ILogEntry entry) => _entry = entry;

        public LogProperty this[string name] => TryGetProperty(name, out var property) ? property : throw DynamicException.Create("PropertyNotFound", $"There is no property with the name '{name}'.");

        public void Push(LogProperty property) => _entry.Push(property);

        public bool TryGetProperty(string name, out LogProperty property)
        {
            return _entry.TryGetProperty(name, out property) && LogProperty.CanProcess.With<T>()(property);
        }

        public IEnumerator<LogProperty> GetEnumerator() => _entry.Where(LogProperty.CanProcess.With<T>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entry).GetEnumerator();
    }
}