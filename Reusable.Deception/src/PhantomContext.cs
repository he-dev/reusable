using System;

namespace Reusable.Deception;

public record struct PhantomContext(params string[] Tags)
{
    public Func<bool>? CanThrow { get; set; } = default!;

    public Func<Exception>? CreateException { get; set; } = default!;

    public static PhantomContext Create<T>(params string[] tags) where T : Exception, new()
    {
        return new(tags) { CreateException = () => new T() };
    }
}