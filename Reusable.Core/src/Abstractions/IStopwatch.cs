using System;

namespace Reusable
{
    public interface IStopwatch
    {
        bool IsRunning { get; }
        TimeSpan Elapsed { get; }
        void Start();
        void Stop();
        void Restart();
        void Reset();
    }
}
