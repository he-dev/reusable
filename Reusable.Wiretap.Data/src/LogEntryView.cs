using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data
{
    public class LogEntryView<T> : ILogEntry where T : class, ILogProperty
    {
        private readonly ILogEntry _entry;

        public LogEntryView(ILogEntry entry) => _entry = entry;

        public ILogProperty this[string name] => 
            TryGetProperty(name, out var property)
                ? property! 
                : throw DynamicException.Create("PropertyNotFound", $"There is no property with the name '{name}'.");

        public ILogEntry Push(ILogProperty property) => _entry.Push(property);

        public bool TryGetProperty(string name, out ILogProperty? property)
        {
            return _entry.TryGetProperty(name, out property) && property is T;
        }

        public IEnumerator<ILogProperty> GetEnumerator() => _entry.Where(property => property is T).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entry).GetEnumerator();
    }
}