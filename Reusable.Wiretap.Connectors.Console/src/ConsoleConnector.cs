using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Connectors;

[PublicAPI]
public abstract class ConsoleConnector<T> : IConnector
{
    public IConsoleMessageBuilder<T> Template { get; set; } = null!;

    public ConsoleStyleSheet Styles { get; set; } = new(new ConsoleStyle.LogLevel.Information())
    {
        new ConsoleStyle.LogLevel(new ConsoleStyle.LogLevel.Information())
        {
            new ConsoleStyle.LogLevel.Trace(),
            new ConsoleStyle.LogLevel.Debug(),
            new ConsoleStyle.LogLevel.Information(),
            new ConsoleStyle.LogLevel.Warning(),
            new ConsoleStyle.LogLevel.Error(),
            new ConsoleStyle.LogLevel.Fatal(),
        }
    };

    public abstract void Log(ILogEntry entry);
}

public interface IConsoleMessageBuilder<out T>
{
    T Build(ILogEntry entry);
}