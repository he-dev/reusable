using System.Collections.Generic;
using Reusable.Essentials.Extensions;

namespace Reusable.Wiretap.Abstractions;

public interface ILogEntry : IEnumerable<ILogProperty>, ITryGetValue<string, object>
{
    ILogProperty this[string name] { get; }

    ILogEntry Push(ILogProperty property);

    bool TryGetProperty(string name, out ILogProperty property);
}