using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
