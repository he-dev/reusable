using System;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerScope : IDisposable, IEnumerable<ILoggerScope>
{
    string Name { get; }
    
    ILoggerNode First { get; }
    
    IDictionary<string, object> Items { get; }
}