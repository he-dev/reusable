namespace Reusable.Wiretap.Data;

public enum TimeSpanUnit
{
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days,

    // ReSharper disable InconsistentNaming
    ms = Milliseconds,
    sec = Seconds,
    min = Minutes,
    h = Hours,

    d = Days
    // ReSharper restore InconsistentNaming
    
}