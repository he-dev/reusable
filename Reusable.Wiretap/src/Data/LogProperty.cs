using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public interface ILogPropertyName { }

public abstract record LogProperty(string Name, object Value) : ILogProperty
{
    public static ILogPropertyName? Names => default;

    public record Obsolete(string Name) : LogProperty<IMetaProperty>(Name, new object());

    public record Null : LogProperty<IMetaProperty>
    {
        private Null() : base(nameof(Null), new object()) { }

        public static readonly Null Instance = new();
    }
}

public record LogProperty<T>(string Name, object Value) : LogProperty(Name, Value), ILogProperty<T> where T : ILogPropertyGroup { }

/// <summary>
/// Specifies a built-in property that is natively supported by the channel.
/// </summary>
public interface IRegularProperty : ILogPropertyGroup { }

/// <summary>
/// Specifies a property that needs to be processed before it can be logged.
/// </summary>
public interface ITransientProperty : ILogPropertyGroup { }

/// <summary>
/// Specifies a property that supports the framework.
/// </summary>
public interface IMetaProperty : ILogPropertyGroup { }