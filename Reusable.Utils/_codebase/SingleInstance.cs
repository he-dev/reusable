using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable
{
    public class SingleInstance : IDisposable
    {
        private readonly Mutex _mutex;
        private readonly bool _ownsMutex;
        private readonly TimeSpan _wait;

        public SingleInstance(string name, TimeSpan wait)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _mutex = new Mutex(true, name, out _ownsMutex);
            _wait = wait;
        }

        public SingleInstance(string name) : this(name, TimeSpan.Zero) { }

        public bool IsRunning()
        {
            return !_mutex.WaitOne(_wait);
        }

        void IDisposable.Dispose()
        {
            if (_ownsMutex)
            {
                _mutex.ReleaseMutex();
            }
            _mutex.Dispose();
        }
    }
}
