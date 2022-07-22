using System;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerFactory
{
    ILogger CreateLogger(string name);
}

