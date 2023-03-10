using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap;

public class Logger : ILogger
{
    public Logger(LogDelegate log) => Log = log;

    private LogDelegate Log { get; }

    [MustUseReturnValue]
    public LoggerContext Start
    (
        string name,
        object? details = default,
        object? attachment = default
    )
    {
        return new LoggerContext(Log, name).Also(context => context.Started(details, attachment));
    }
}