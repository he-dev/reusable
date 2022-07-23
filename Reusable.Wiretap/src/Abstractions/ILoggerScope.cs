using System;
using System.Collections.Generic;
using Reusable.Essentials;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerScope : ILoggerMiddleware, IDisposable
{
    IEnumerable<ILoggerScope> Parents { get; }

    /// <summary>
    /// Stores objects associated with this scope.
    /// </summary>
    IDictionary<string, object> Properties { get; }
}