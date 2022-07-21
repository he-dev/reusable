using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public interface ILogPropertyName { }

public abstract record LogProperty(string Name, object Value) : ILogProperty
{
    //public static readonly ILogProperty Empty = new LogProperty<IMetaProperty>(nameof(Empty), new object());

    public static ILogPropertyName? Names => default;

    public record Empty : LogProperty<IMetaProperty>
    {
        private Empty() : base(nameof(Empty), new object()) { }

        public static readonly Empty Instance = new();
    }

    public record Obsolete(string Name) : LogProperty<IMetaProperty>(Name, new object());
}

public record LogProperty<T>(string Name, object Value) : LogProperty(Name, Value), ILogProperty<T> where T : ILogPropertyTag { }

/// <summary>
/// Specifies a built-in property that is natively supported by the channel.
/// </summary>
public interface IRegularProperty : ILogPropertyTag { }

/// <summary>
/// Specifies a property that needs to be processed before it can be logged.
/// </summary>
public interface ITransientProperty : ILogPropertyTag { }

/// <summary>
/// Specifies a property that supports the framework.
/// </summary>
public interface IMetaProperty : ILogPropertyTag { }


// public record LoggableProperty(string Name, object Value) : LogProperty(Name, Value), ILoggableProperty
// {
//     public record Environment(object Value) : LoggableProperty(nameof(Environment), Value);
//
//     public record Product(object Value) : LoggableProperty(nameof(Product), Value);
//
//     public record Timestamp(object Value) : LoggableProperty(nameof(Timestamp), Value);
//
//     public record Logger(object Value) : LoggableProperty(nameof(Logger), Value);
//
//     public record Level(object Value) : LoggableProperty(nameof(Level), Value);
//
//     public record Message(object Value) : LoggableProperty(nameof(Message), Value);
//
//     public record Exception(object Value) : LoggableProperty(nameof(Exception), Value);
//
//     public record Elapsed(object Value) : LoggableProperty(nameof(Elapsed), Value);
//
//     public record Member(object Value) : LoggableProperty(nameof(Member), Value);
//
//     public record Category(object Value) : LoggableProperty(nameof(Category), Value);
//
//     public record Layer(object Value) : LoggableProperty(nameof(Layer), Value);
//
//     public record CallerMemberName(object Value) : LoggableProperty(nameof(CallerMemberName), Value);
//
//     public record CallerLineNumber(object Value) : LoggableProperty(nameof(CallerLineNumber), Value);
//
//     public record CallerFilePath(object Value) : LoggableProperty(nameof(CallerFilePath), Value);
// }
//
// public record DataProperty(string Name, object Value) : LogProperty(Name, Value)
// {
//     public record Correlation(object Value) : DataProperty(nameof(Correlation), Value);
//
//     public record Snapshot(object Value) : DataProperty(nameof(Snapshot), Value);
//
//     public record Scope(object Value) : MetaProperty(nameof(Scope), Value);
// }
//
// public record DynamicProperty(int Id, object Value) : LogProperty($"{nameof(DynamicProperty)}{Id}", Value);
//
// public record ActionProperty(object Value) : LogProperty(nameof(ActionProperty), Value);
//
// public abstract record DestructibleProperty(string Name, object Value) : LogProperty(Name, Value)
// {
//     public record Object(object Value) : DestructibleProperty(nameof(Object), Value);
// }
//
// public abstract record MetaProperty(string Name, object Value) : LogProperty(Name, Value)
// {
//     public record Empty() : MetaProperty(nameof(Empty), default);
//
//     public record Deleted(string Name) : MetaProperty(Name, default);
//
//     public record OverrideBuffer() : MetaProperty(nameof(OverrideBuffer), default);
//
//     public record PopulateExecution() : MetaProperty(nameof(PopulateExecution), default);
// }