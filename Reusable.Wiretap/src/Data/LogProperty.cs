using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public interface ILogPropertyName { }

public abstract record LogProperty(string Name, object Value) : ILogProperty
{
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

