using System;

namespace Reusable.Marbles;

public static class Lazy
{
    public static Lazy<T> Create<T>(Func<T> valueFactory) => new(valueFactory);
}