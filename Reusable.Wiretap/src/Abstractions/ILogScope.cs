using System;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILogScope : IDisposable, IEnumerable<ILogScope>
{
    ILoggerNode First { get; }

    Stack<(Exception Exception, ILogCaller Caller)> Exceptions { get; }
}