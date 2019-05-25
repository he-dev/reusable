using System.Diagnostics;

namespace Reusable.Data
{
    // ReSharper disable once UnusedTypeParameter - 'T'  is required.
    public interface INamespace<out T> where T : INamespace { }

    public static class Use<T> where T : INamespace
    {
        [DebuggerNonUserCode]
        public static INamespace<T> Namespace => default;
    }

    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }
}