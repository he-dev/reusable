using System;

namespace Reusable.Essentials;

public class DateTimeUtc : IDateTime
{
    public DateTime Now() => DateTime.UtcNow;
}