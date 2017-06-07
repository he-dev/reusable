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
        private readonly Action<T> _onDispose;
        private T _state;
        private bool _disposed;
        private bool _autoDispose;

        public Usingifier(Func<T> initialize, Action<T> onDispose, bool autoDispose = false)
        {
            _initialize = initialize ?? throw new ArgumentNullException(nameof(initialize));
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
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

                if (_state != null)
                {
                    _onDispose(_state);
                    if (_state is IDisposable disposable && _autoDispose)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }

    public static class UsingifierExtensions
    {
        public static Usingifier<T> Usingify<T>(this T obj, Action<T> onDispose, bool autoDispose = false) where T : class
        {
            return new Usingifier<T>(() => obj, onDispose, autoDispose);
        }
    }
}
