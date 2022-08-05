using System;

namespace Reusable.Marbles;

public class DateTimeUtc : IDateTime
{
    public DateTime Now() => DateTime.UtcNow;
}