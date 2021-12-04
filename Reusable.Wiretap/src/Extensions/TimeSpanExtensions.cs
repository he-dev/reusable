using System;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Extensions
{
    public static class TimeSpanExtensions
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
                _ => timeSpan.TotalMilliseconds
            };
        }
    }
}