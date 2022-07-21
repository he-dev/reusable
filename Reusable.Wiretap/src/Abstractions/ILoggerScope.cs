using System;
using System.Collections.Generic;
using Reusable.Essentials;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerScope : IDisposable, IEnumerable<ILoggerNode>
{
    IEnumerable<ILoggerScope> Parents { get; }

    /// <summary>
    /// Stores objects associated with this scope.
    /// </summary>
    IDictionary<string, object> Items { get; }

    void Invoke(ILogEntry entry);
}