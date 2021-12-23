using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node creates properties using factories specified in <c>TryCreateProperties</c>.
/// </summary>
public class GuessProperties : LoggerNode
{
    public List<ITryGuessProperty> Factories { get; set; } = new();

    public override void Invoke(ILogEntry entry)
    {
        if (entry.TryGetProperty(nameof(GuessableProperty.Unknown), out var unknown) && unknown?.Value is IEnumerable<object> items)
        {
            var funcs =
                from item in items
                where item is { }
                from tryGuessProperty in Factories
                from func in tryGuessProperty.Invoke(item)
                select func;

            foreach (var func in funcs)
            {
                func(entry);
            }
        }

        //    throw DynamicException.Create("UnsupportedObject", $"Could not create properties from '{obj.GetType().ToPrettyString()}'.");
        InvokeNext(entry);
    }
}

public interface ITryGuessProperty
{
    IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj);
}

public class TryGuessEnum : ITryGuessProperty
{
    public IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj)
    {
        if (obj.GetType() is { IsEnum: true } type)
        {
            // Don't ToString the value because it will break the log-level.
            //var name = type.GetCustomAttribute<PropertyNameAttribute>()?.ToString() ?? type.Name;
            yield return entry => entry.Push(new LoggableProperty(type.Name, obj));
        }
    }
}

public class TryGuessException : ITryGuessProperty
{
    public IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj)
    {
        if (obj is Exception exception)
        {
            yield return entry => entry.Push(new LoggableProperty.Exception(exception));
        }
    }
}

public class TryGuessEntryAction : ITryGuessProperty
{
    public IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj)
    {
        if (obj is Action<ILogEntry> action)
        {
            yield return entry => entry.Also(action);
        }
    }
}

public class TryGuessEntryActions : ITryGuessProperty
{
    public IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj)
    {
        if (obj is IEnumerable<Action<ILogEntry>> actions)
        {
            yield return entry =>
            {
                foreach (var action in actions)
                {
                    entry.Also(action);
                }

                return entry;
            };
        }
    }
}

public class TryGuessProperty : ITryGuessProperty
{
    public IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj)
    {
        if (obj is ILogProperty property)
        {
            yield return entry => entry.Push(property);
        }
    }
}

public class TryGuessProperties : ITryGuessProperty
{
    public IEnumerable<Func<ILogEntry, ILogEntry>> Invoke(object obj)
    {
        if (obj is IEnumerable<ILogProperty> properties)
        {
            yield return entry =>
            {
                foreach (var property in properties)
                {
                    entry.Push(property);
                }

                return entry;
            };
        }
    }
}