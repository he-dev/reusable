using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable
{
    //[PublicAPI]
    //public class Disposable<TState> : IDisposable where TState : class
    //{
    //    private bool _isInitialized;
    //    private readonly Func<TState> _initialize;
    //    private readonly Action<TState> _dispose;
    //    private TState _state;
    //    private bool _disposed;

    //    private Disposable([NotNull] Func<TState> initialize, [NotNull] Action<TState> dispose)
    //    {
    //        _initialize = initialize ?? throw new ArgumentNullException(nameof(initialize));
    //        _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
    //    }

    //    [NotNull]
    //    public TState Value => _state ?? throw new InvalidOperationException($"{nameof(Disposable<TState>)} is not initialized.");

    //    public static Disposable<TState> Create([NotNull] Func<TState> initialize, [NotNull] Action<TState> dispose)
    //    {
    //        if (initialize == null) throw new ArgumentNullException(nameof(initialize));
    //        if (dispose == null) throw new ArgumentNullException(nameof(dispose));

    //        return new Disposable<TState>(initialize, dispose);
    //    }                

    //    public static Disposable<TState> CreateInitialized([NotNull] Func<TState> initialize, [NotNull] Action<TState> dispose)
    //    {
    //        if (initialize == null) throw new ArgumentNullException(nameof(initialize));
    //        if (dispose == null) throw new ArgumentNullException(nameof(dispose));

    //        return new Disposable<TState>(initialize, dispose).Initialize();
    //    }

    //    public Disposable<TState> Initialize()
    //    {
    //        if (_isInitialized)
    //        {
    //            throw new InvalidOperationException($"Disposable state is already initialized.");
    //        }
    //        _state = _initialize();
    //        _isInitialized = true;
    //        return this;
    //    }

    //    public virtual void Dispose()
    //    {
    //        if (_disposed)
    //        {
    //            return;
    //        }

    //        _disposed = true;

    //        if (_state == null)
    //        {
    //            return;
    //        }

    //        _dispose(_state);

    //        if (_state is IDisposable disposable)
    //        {
    //            disposable.Dispose();
    //        }
    //    }
    //}

    public class Disposable : IDisposable
    {
        private readonly Action _dispose;

        private Disposable(Action dispose)
        {
            _dispose = dispose;
        }

        public static IDisposable Empty => new Disposable(() => { });

        public static IDisposable Create(Action dispose)
        {
            return new Disposable(dispose);
        }

        public void Dispose()
        {
            _dispose();
        }
    }

    //public static class DisposableExtensions
    //{
    //    public static Disposable<TState> ToDisposable<TState>(this TState state, Action<TState> dispose) where TState : class
    //    {
    //        return Disposable<TState>.Create(() => state, dispose);
    //    }
    //}
}
