using System;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerFactory : IDisposable
{
    ILogger CreateLogger(string name);
}

