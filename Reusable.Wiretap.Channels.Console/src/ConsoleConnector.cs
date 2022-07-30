using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

[PublicAPI]
public abstract class ConsoleChannel<T> : Channel<ConsoleChannel<T>>
{
    protected ConsoleChannel(string? name) : base(name) { }
    
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

}

public interface IConsoleMessageBuilder<out T>
{
    T Build(ILogEntry entry);
}