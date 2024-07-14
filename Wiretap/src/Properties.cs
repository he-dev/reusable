using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Wiretap;

public interface IPropertyProvider
{
    string Name { get; }
    object? Create(Telemetry.Item item);
}

public class PropertyFactory : List<IPropertyProvider>
{
    public IEnumerable<KeyValuePair<string, object>> Create(Telemetry.Item item)
    {
        foreach (var provider in this)
        {
            // Create a property and yield it only if it's not-null.
            if (provider.Create(item) is { } property)
            {
                yield return new KeyValuePair<string, object>(provider.Name, property);
            }
        }
    }
}

public class ExecutionProperty(Procedure procedure)
{
    public Guid Id => procedure.Last().Id;
    public IEnumerable<string> Path => procedure.Select(x => x.Name);
    public double Elapsed => procedure.Last().Elapsed.TotalSeconds;

    public class Provider(string name = "execution") : IPropertyProvider
    {
        public string Name => name;

        public object? Create(Telemetry.Item item) => new ExecutionProperty(item.Procedure);
    }
}

public class ProcedureProperty(Procedure procedure)
{
    public Guid Id => procedure.Id;
    public string Name => procedure.Name;
    public DataCollection Data => new(procedure.Select(x => x.Data));

    public IEnumerable<string> Tags =>
        procedure
            .Select(x => x.Tags)
            .Aggregate(Enumerable.Empty<string>(), (acc, tags) => acc.Concat(tags))
            .ToHashSet(new StringComparerLite());

    public double Elapsed => procedure.Elapsed.TotalSeconds;
    public int Depth => procedure.Count();

    public class Provider(string name = "procedure") : IPropertyProvider
    {
        public string Name => name;

        public object? Create(Telemetry.Item item) => new ProcedureProperty(item.Procedure);
    }
}

public record DataCollection(IEnumerable<object?> Data) : IEnumerable<object>
{
    [MustDisposeResource]
    public IEnumerator<object> GetEnumerator()
    {
        var items =
            from x in Data
            where x is not null
            select x;

        return items.GetEnumerator();
    }

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
}

public class TraceProperty(Trace trace)
{
    public string Name => trace.Name;
    public string? Message => trace.Message;
    public object? Data => trace.Data;

    public IEnumerable<object> Tags =>
        trace
            .Tags
            .ToHashSet(new StringComparerLite());

    public class Provider(string name = "trace") : IPropertyProvider
    {
        public string Name => name;

        public object? Create(Telemetry.Item item) => new TraceProperty(item.Trace);
    }
}

public class ExceptionProperty(Exception exception)
{
    public string Type => exception.GetType().Name;
    public string Message => exception.Message;
    public string StackTrace => exception.ToString();

    public class Provider(string name = "exception") : IPropertyProvider
    {
        public string Name => name;

        public object? Create(Telemetry.Item item)
        {
            return item.Exception switch
            {
                { } e => new ExceptionProperty(e),
                _ => default
            };
        }
    }
}

public class EnvironmentProperty(IEnumerable<string> names)
    : Dictionary<string, string?>(names.Select(x => new KeyValuePair<string, string?>(x, Environment.GetEnvironmentVariable(x))))
{
    public class Provider(string name = "environment") : List<string>, IPropertyProvider
    {
        public string Name => name;

        public object? Create(Telemetry.Item item) => new EnvironmentProperty(this);
    }
}

public class SourceProperty(Telemetry.Item item)
{
    public string? Type => item.Type?.Name;
    public string Func => item.Procedure.Source.Func;
    public string File => item.Procedure.Source.File;
    public int Line => item.Procedure.Source.Line;

    public class Provider(string name = "source") : IPropertyProvider
    {
        public string Name => name;

        public object? Create(Telemetry.Item item)
        {
            return (item.Procedure.Traces == 1) ? new SourceProperty(item) : default;
        }
    }
}