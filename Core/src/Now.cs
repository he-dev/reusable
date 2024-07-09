using System;

namespace Reusable;

public interface INow<out T>
{
    public Func<T> Get { get; }
}

public class Now<T>(Func<T> get) : INow<T>
{
    public Func<T> Get { get; } = get;
}

public static class Now
{
    public static INow<T> By<T>(Func<T> get) => new Now<T>(get);

    public static INow<DateTime> Utc => By(() => DateTime.UtcNow);

    public static INow<DateTime> Local => By(() => DateTime.Now);

    public static class Offset
    {
        public static INow<DateTimeOffset> Utc => By(() => DateTimeOffset.UtcNow);

        public static INow<DateTimeOffset> Local => By(() => DateTimeOffset.Now);
    }
}