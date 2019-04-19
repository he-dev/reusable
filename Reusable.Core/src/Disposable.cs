using System;

namespace Reusable
{
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
}
