using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class Usingifier<T> : IDisposable where T : class
    {
        private readonly Func<T> _initialize;
        private readonly Action<T> _cleanUp;
        private T _state;
        private bool _disposed;
        private bool _autoDispose;

        public Usingifier(Func<T> initialize, Action<T> cleanUp, bool autoDispose = false)
        {
            _initialize = initialize ?? throw new ArgumentNullException(nameof(initialize));
            _cleanUp = cleanUp ?? throw new ArgumentNullException(nameof(cleanUp));
            _autoDispose = autoDispose;
        }

        public Usingifier<T> Initialize()
        {
            _state = _initialize();
            return this;
        }

        public virtual void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (_state != null) _cleanUp(_state);
                if (_state is IDisposable disposable && _autoDispose) disposable.Dispose();
            }
        }
    }

    public static class UsingifierExtensions
    {
        public static Usingifier<T> Usingify<T>(this T obj, Action<T> cleanUp, bool autoDispose = false) where T : class
        {
            return new Usingifier<T>(() => obj, cleanUp, autoDispose);
        }
    }
}
