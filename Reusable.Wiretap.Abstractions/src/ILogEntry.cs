using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions
{
    public interface ILogEntry : IEnumerable<ILogProperty>
    {
        ILogProperty this[string name] { get; }

        ILogEntry Push(ILogProperty property);

        bool TryGetProperty(string name, out ILogProperty? property);
    }
}