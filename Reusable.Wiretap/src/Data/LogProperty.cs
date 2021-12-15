using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public abstract record LogProperty(string Name, object Value) : ILogProperty;

public record LoggableProperty(string Name, object Value) : LogProperty(Name, Value), ILoggableProperty
{
    public record Environment(object Value) : LoggableProperty(nameof(Environment), Value);
    
    public record Product(object Value) : LoggableProperty(nameof(Product), Value);
        
    public record Timestamp(object Value) : LoggableProperty(nameof(Timestamp), Value);

    public record Logger(object Value) : LoggableProperty(nameof(Logger), Value);

    public record Level(object Value) : LoggableProperty(nameof(Level), Value);

    public record Message(object Value) : LoggableProperty(nameof(Message), Value);

    public record Exception(object Value) : LoggableProperty(nameof(Exception), Value);

    public record Elapsed(object Value) : LoggableProperty(nameof(Elapsed), Value);
        
    public record Member(object Value) : LoggableProperty(nameof(Member), Value);
        
    public record Category(object Value) : LoggableProperty(nameof(Category), Value);
    
    public record Layer(object Value) : LoggableProperty(nameof(Layer), Value);

    public record CallerMemberName(object Value) : LoggableProperty(nameof(CallerMemberName), Value);

    public record CallerLineNumber(object Value) : LoggableProperty(nameof(CallerLineNumber), Value);

    public record CallerFilePath(object Value) : LoggableProperty(nameof(CallerFilePath), Value);
}

public record SerializableProperty(string Name, object Value) : LogProperty(Name, Value)
{
    public record Correlation(object Value) : SerializableProperty(nameof(Correlation), Value);

    public record Snapshot(object Value) : SerializableProperty(nameof(Snapshot), Value);
}

public abstract record GuessableProperty(string Name, object Value) : LogProperty(Name, Value)
{
    public record Unknown(object Value) : GuessableProperty(nameof(Unknown), Value);
}

public abstract record CallableProperty(string Name, object Value) : LogProperty(Name, Value)
{
    public record EntryAction(object Value) : CallableProperty(nameof(EntryAction), Value);
}

public abstract record DestructibleProperty(string Name, object Value) : LogProperty(Name, Value)
{
    public record Object(object Value) : DestructibleProperty(nameof(Object), Value);
}

public abstract record RenderableProperty(string Name, object Value) : LogProperty(Name, Value)
{
    public record Html(object Value) : RenderableProperty(nameof(Html), Value);
}

public abstract record MetaProperty(string Name, object Value) : LogProperty(Name, Value)
{
    public record OverrideBuffer() : MetaProperty(nameof(OverrideBuffer), default);

    public record Scope(object Value) : MetaProperty(nameof(Scope), Value);
    
    public record PopulateExecution() : MetaProperty(nameof(PopulateExecution), default);
}


public static class LogPropertyExtensions
{
    public static T? ValueOrDefault<T>(this ILogProperty? property, T? defaultValue = default)
    {
        return property?.Value switch { T t => t, _ => defaultValue };
    }
}