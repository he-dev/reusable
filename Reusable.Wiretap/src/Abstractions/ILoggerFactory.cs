using System;
using JetBrains.Annotations;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerFactory
{
    [MustUseReturnValue]
    ILogger CreateLogger(string name);
}

