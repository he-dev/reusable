using System;
using System.Diagnostics;

namespace Reusable.Marbles;

public class SystemStopwatch : IStopwatch
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public bool IsRunning => _stopwatch.IsRunning;

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public static IStopwatch StartNew() => new SystemStopwatch().Also(x => x.Start());

    public void Start() => _stopwatch.Start();

    public void Stop() => _stopwatch.Stop();

    public void Restart() => _stopwatch.Restart();

    public void Reset() => _stopwatch.Restart();

    public override string ToString() => _stopwatch.Elapsed.ToString();
}