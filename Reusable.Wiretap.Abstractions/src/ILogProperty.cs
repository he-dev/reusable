using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILogProperty
{
    string Name { get; }

    object Value { get; }
}