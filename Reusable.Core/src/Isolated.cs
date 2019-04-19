using System;
using JetBrains.Annotations;

namespace Reusable
{
#if NET47
    [PublicAPI]
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject, new()
    {
        private readonly AppDomain _domain;

        public Isolated() : this(AppDomain.CurrentDomain.SetupInformation) { }

        public Isolated([NotNull] AppDomainSetup setup)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            _domain = AppDomain.CreateDomain($"Isolated-{Guid.NewGuid()}", null, setup);
            Value = (T)_domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        [NotNull]
        public T Value { get; }

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }
    }
#endif
}