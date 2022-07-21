namespace Reusable.Wiretap.Abstractions;

public interface ILogProperty
{
    string Name { get; }

    object Value { get; }
}

public interface ILogPropertyTag { }

public interface ILogProperty<T> : ILogProperty where T : ILogPropertyTag { }

public interface ILoggableProperty : ILogProperty { }

public interface IKnownProperty : ILogProperty { }