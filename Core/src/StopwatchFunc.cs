using System;
using JetBrains.Annotations;

namespace Reusable;

[PublicAPI]
public class StopwatchFunc(Func<TimeSpan> elapsed, Action reset) :  IStopwatch
{
    private TimeSpan Last { get; set; }

    public bool IsRunning { get; private set; } = true;

    public TimeSpan Elapsed => IsRunning ? Last = elapsed() : Last;

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    public void Restart()
    {
        Stop();
        Reset();
        Start();
    }

    public void Reset() => reset();
}