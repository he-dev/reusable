using System;

namespace Reusable.Marbles;

public interface IStopwatch
{
    bool IsRunning { get; }
    TimeSpan Elapsed { get; }
    void Start();
    void Stop();
    void Restart();
    void Reset();
}