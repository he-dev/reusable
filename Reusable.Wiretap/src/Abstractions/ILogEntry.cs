using System.Collections.Generic;
using Reusable.Essentials.Extensions;

namespace Reusable.Wiretap.Abstractions;

public interface ILogEntry : IEnumerable<ILogProperty>, ITryGetValue<string, object>
{
    ILogProperty this[string name] { get; }

    ILogEntry Push(ILogProperty property);

    /// <summary>
    /// Tries to get the latest version of a property.
    /// </summary>
    bool TryPeek(string name, out ILogProperty property);

    IEnumerable<ILogProperty> Versions(string name);
}