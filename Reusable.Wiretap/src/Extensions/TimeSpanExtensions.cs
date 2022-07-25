using System;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Extensions;

public static class TimeSpanExtensions
{
    public static double ToDouble(this TimeSpan timeSpan, TimeSpanUnit unit)
    {
        return unit switch
        {
            TimeSpanUnit.Milliseconds => timeSpan.TotalMilliseconds,
            TimeSpanUnit.Seconds => timeSpan.TotalSeconds,
            TimeSpanUnit.Minutes => timeSpan.TotalMinutes,
            TimeSpanUnit.Hours => timeSpan.TotalHours,
            TimeSpanUnit.Days => timeSpan.TotalDays,
            _ => timeSpan.TotalMilliseconds
        };
    }
}