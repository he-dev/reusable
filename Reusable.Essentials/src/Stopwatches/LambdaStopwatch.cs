using System;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials;

[PublicAPI]
public class LambdaStopwatch : IStopwatch
{
    private readonly Func<TimeSpan> _elapsed;

    private readonly Action _reset;

    private TimeSpan _last;

    public LambdaStopwatch(Func<TimeSpan> elapsed, Action reset)
    {
        _elapsed = elapsed;
        _reset = reset;
    }

    public bool IsRunning { get; private set; }

    public TimeSpan Elapsed => IsRunning ? _last = _elapsed() : _last;

    public static LambdaStopwatch StartNew(Func<TimeSpan> elapsed, Action reset) => new LambdaStopwatch(elapsed, reset).Also(x => x.Start());

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    public void Restart()
    {
        Stop();
        Reset();
        Start();
    }

    public void Reset() => _reset();
}