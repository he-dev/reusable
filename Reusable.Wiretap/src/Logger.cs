using System.Collections;
using System.Collections.Generic;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap;

public class Logger : ILogger
{
    private ILoggerNode First { get; }

    public Logger(ILoggerNode first) => First = first;

    public void Log(ILogEntry logEntry) => First.Invoke(logEntry);

    public static IEnumerable<ILoggerNode> Pipeline<T>() where T : LoggerPipeline, new() => new T();

    public IEnumerator<ILoggerNode> GetEnumerator() => First.EnumerateNext().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        foreach (var node in First.EnumerateNext())
        {
            node.Dispose();
        }
    }

    public class Empty : Logger
    {
        private Empty() : base(LoggerNode.Empty.Instance) { }

        public static readonly ILogger Instance = new Empty();
    }
}

// This logger supports DI.
public class Logger<T> : ILogger<T>
{
    // This constructor makes it easier to create a typed logger with DI.
    public Logger(ILoggerFactory loggerFactory) => Instance = loggerFactory.CreateLogger(typeof(T).ToPrettyString());

    private ILogger Instance { get; }

    public void Log(ILogEntry logEntry) => Instance.Log(logEntry);


    public IEnumerator<ILoggerNode> GetEnumerator() => Instance.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose() => Instance.Dispose();
}