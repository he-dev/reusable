using System;

namespace Reusable.Essentials;

public class DateTimeLambda : IDateTime
{
    private readonly Func<DateTime> _now;

    public DateTimeLambda(Func<DateTime> now) => _now = now;

    public DateTime Now() => _now();
}