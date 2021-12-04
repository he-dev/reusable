using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data
{
    public class LogEntryView<T> : ILogEntry where T : class, ILogProperty
    {
        public LogEntryView(ILogEntry parent) => Parent = parent;

        private ILogEntry Parent { get; }

        public ILogProperty this[string name] => Parent[name];

        public ILogEntry Push(ILogProperty property) => Parent.Push(property);

        public bool TryGetProperty(string name, out ILogProperty property) => Parent.TryGetProperty(name, out property) && property is T;

        public IEnumerator<ILogProperty> GetEnumerator() => Parent.Where(property => property is T).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Parent).GetEnumerator();
    }
}