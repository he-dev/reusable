using System.Diagnostics;

namespace Reusable.Data
{
    // ReSharper disable once UnusedTypeParameter - 'T'  is required.
    public interface INamespace<out T> { }

    public static class Use<T>
    {
        [DebuggerNonUserCode]
        public static INamespace<T> Namespace => default;
    }

    public interface INamespace { }
}