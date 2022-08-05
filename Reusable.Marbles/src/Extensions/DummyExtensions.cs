using JetBrains.Annotations;

namespace Reusable.Marbles.Extensions;

public static class DummyExtensions
{
    [CanBeNull]
    public static T Noop<T>([CanBeNull] this T obj) => obj;
}