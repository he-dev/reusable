using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

public class GuessProperty : LoggerMiddleware
{
    public override void Invoke(ILogEntry entry)
    {
        var update = LogEntry.Empty();
        foreach (var property in entry.OfType<ITransientProperty>())
        {
            if (TryGuess(property.Value, out var result))
            {
                update.Push(result);
                update.Push(new LogProperty.Obsolete(property.Name));
            }
        }

        entry.Push(update);
        Next?.Invoke(entry);
    }

    protected virtual bool TryGuess(object value, out ILogProperty result)
    {
        result = value as ILogProperty ?? LogProperty.Null.Instance;
        return result is not LogProperty.Null;
    }
}

public class GuessEnum : GuessProperty
{
    protected override bool TryGuess(object value, out ILogProperty result)
    {
        result =
            value.GetType() is { IsEnum: true } type
                ? new LogProperty<IRegularProperty>(type.Name, value)
                : LogProperty.Null.Instance;

        return result is not LogProperty.Null;
    }
}

public class GuessException : GuessProperty
{
    protected override bool TryGuess(object value, out ILogProperty result)
    {
        result =
            value is Exception exception
                ? new LogProperty<IRegularProperty>(nameof(exception), exception)
                : LogProperty.Null.Instance;

        return result is not LogProperty.Null;
    }
}

public class GuessMessage : GuessProperty
{
    protected override bool TryGuess(object value, out ILogProperty result)
    {
        result =
            value is string message
                ? new LogProperty<IRegularProperty>(nameof(message), message)
                : LogProperty.Null.Instance;

        return result is not LogProperty.Null;
    }
}