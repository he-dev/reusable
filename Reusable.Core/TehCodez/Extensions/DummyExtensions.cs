using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class DummyExtensions
    {
        [CanBeNull]
        public static T Noop<T>([CanBeNull] this T obj) => obj;
    }
}
