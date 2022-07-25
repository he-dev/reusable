namespace Reusable.Wiretap.Abstractions;

public interface ILogProperty
{
    string Name { get; }

    object Value { get; }
}

public interface ILogPropertyGroup { }

public interface ILogProperty<T> : ILogProperty where T : ILogPropertyGroup { }