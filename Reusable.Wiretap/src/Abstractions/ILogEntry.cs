using System.Collections.Generic;
using Reusable.Essentials.Extensions;

namespace Reusable.Wiretap.Abstractions;

public interface ILogEntry : IEnumerable<ILogProperty>, ITryGetValue<string, object>
{
    /// <summary>
    /// Gets the latest version of a property or an empty one.
    /// </summary>
    ILogProperty this[string name] { get; }

    /// <summary>
    /// Pushes a new version of a property.
    /// </summary>
    ILogEntry Push(ILogProperty property);

    /// <summary>
    /// Tries to get the latest version of a property.
    /// </summary>
    bool TryPeek(string name, out ILogProperty property);

    /// <summary>
    /// Gets all versions of the specified property.
    /// </summary>
    IEnumerable<ILogProperty> Versions(string name);
}