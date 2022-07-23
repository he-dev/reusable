using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap;

public class Logger : LoggerMiddleware, ILogger
{
    public Logger(string name) => Name = name;

    #region LoggerNode

    public override void Invoke(ILogEntry entry) => Next?.Invoke(entry.Push<IRegularProperty>(nameof(Logger), Name));

    #endregion

    #region ILogger

    public string Name { get; }

    public void Log(ILogEntry entry) => Invoke(entry);

    #endregion

    public new class Empty : Logger
    {
        private Empty() : base(nameof(Empty)) { }

        public static readonly ILogger Instance = new Empty();
    }
}

// This logger supports DI.
public class Logger<T> : Logger, ILogger<T>
{
    // This constructor makes it easier to create a typed logger with DI.
    public Logger(string name) : base(name) { }
}