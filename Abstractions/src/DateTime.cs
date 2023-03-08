using System;

namespace Reusable;

public interface INow<out T>
{
    public Func<T> Get { get; }
}

public class Now<T> : INow<T>
{
    public Now(Func<T> get) => Get = get;

    public Func<T> Get { get; }
}

public static class Now
{
    public static INow<T> By<T>(Func<T> get) => new Now<T>(get);
}


public interface IDateTime
{
    DateTime Now();
}

public interface IDateTimeOffset
{
    DateTimeOffset Now();
}

public class DateTimeUtc : IDateTime
{
    public DateTime Now() => DateTime.UtcNow;
}

public class DateTimeLocal : IDateTime
{
    public DateTime Now() => DateTime.Now;
}

public class DateTimeOffsetUtc : IDateTimeOffset
{
    public DateTimeOffset Now() => DateTimeOffset.UtcNow;
}

public class DateTimeOffsetLocal : IDateTimeOffset
{
    public DateTimeOffset Now() => DateTimeOffset.Now;
}