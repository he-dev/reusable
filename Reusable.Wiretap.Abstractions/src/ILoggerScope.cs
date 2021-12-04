using System;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerScope : IDisposable, IEnumerable<ILoggerScope>
{
    ILoggerNode First { get; }

    Stack<(Exception Exception, ILogCaller Caller)> Exceptions { get; }
}