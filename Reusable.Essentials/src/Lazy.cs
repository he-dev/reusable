using System;

namespace Reusable.Essentials;

public static class Lazy
{
    public static Lazy<T> Create<T>(Func<T> valueFactory) => new(valueFactory);
}