using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data
{
    public abstract record LogProperty(string Name, object Value) : ILogProperty
    {
        
        public static class Names
        {
            public const string Timestamp = nameof(Timestamp);
            public const string Environment = nameof(Environment);
            public const string Correlation = nameof(Correlation);
            public const string Layer = nameof(Layer);
            public const string Logger = nameof(Logger);
            public const string Level = nameof(Level);
            public const string Category = nameof(Category);
            public const string Unit = nameof(Unit);
            public const string Snapshot = nameof(Snapshot);
            public const string Elapsed = nameof(Elapsed);
            public const string Message = nameof(Message);
            public const string Exception = nameof(Exception);
            public const string CallerMemberName = nameof(CallerMemberName);
            public const string CallerLineNumber = nameof(CallerLineNumber);
            public const string CallerFilePath = nameof(CallerFilePath);
            public const string Priority = nameof(Priority);
        }
    }

    public record LoggableProperty(string Name, object Value) : LogProperty(Name, Value)
    {
        public record Timestamp(object Value) : LoggableProperty(nameof(Timestamp), Value);
        public record Logger(object Value) : LoggableProperty(nameof(Logger), Value);
        public record Level(object Value) : LoggableProperty(nameof(Level), Value);
        public record Message(object Value) : LoggableProperty(nameof(Message), Value);
        public record Exception(object Value) : LoggableProperty(nameof(Exception), Value);
        public record Elapsed(object Value) : LoggableProperty(nameof(Elapsed), Value);
        
        public record CallerMemberName(object Value) : LoggableProperty(nameof(CallerMemberName), Value);
        public record CallerLineNumber(object Value) : LoggableProperty(nameof(CallerLineNumber), Value);
        public record CallerFilePath(object Value) : LoggableProperty(nameof(CallerFilePath), Value);
    }

    public record SerializableProperty(string Name, object Value) : LogProperty(Name, Value)
    {
        public record Correlation(object Value) : LoggableProperty(nameof(Correlation), Value);
        
    }

    public record GuessableProperty(string Name, object Value) : LogProperty(Name, Value)
    {
        public record Unknown(object Value) : LoggableProperty(nameof(Unknown), Value);
        
    }

    public record DestructibleProperty(string Name, object Value) : LogProperty(Name, Value);

    public record HtmlProperty(string Name, object Value) : LogProperty(Name, Value)
    {
        public record Message(object Value) : LoggableProperty(nameof(Message), Value);
    }

    public abstract class Names
    {
        public abstract class Categories
        {
            public const string Variable = nameof(Variable);
            public const string Property = nameof(Property);
            public const string Argument = nameof(Argument);
            public const string Meta = nameof(Meta);
            public const string Flow = nameof(Flow);
            public const string Step = nameof(Step);
            public const string Counter = nameof(Counter);
            public const string WorkItem = nameof(WorkItem);
            public const string Routine = nameof(Routine);
        }
    }

    public static class LogPropertyExtensions
    {
        public static T? ValueOrDefault<T>(this ILogProperty? property, T? defaultValue = default)
        {
            return property?.Value switch { T t => t, _ => defaultValue };
        }
    }
}