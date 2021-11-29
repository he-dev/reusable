using System;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Utilities
{
    public static class TimeSpanUtility
    {
        public static double ToDouble(this TimeSpan timeSpan, TimeSpanPrecision precision)
        {
            return precision switch
            {
                TimeSpanPrecision.Milliseconds => timeSpan.TotalMilliseconds,
                TimeSpanPrecision.Seconds => timeSpan.TotalSeconds,
                TimeSpanPrecision.Minutes => timeSpan.TotalMinutes,
                TimeSpanPrecision.Hours => timeSpan.TotalHours,
                TimeSpanPrecision.Days => timeSpan.TotalDays,
                _ => timeSpan.ToDouble(TimeSpanPrecision.Milliseconds)
            };
        }
    }
}