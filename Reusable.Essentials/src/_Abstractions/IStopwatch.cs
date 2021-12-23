using System;

namespace Reusable.Essentials;

public interface IStopwatch
{
    bool IsRunning { get; }
    TimeSpan Elapsed { get; }
    void Start();
    void Stop();
    void Restart();
    void Reset();
}