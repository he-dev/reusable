using System;

namespace Reusable
{
    public class DateTimeUtc : IDateTime
    {
        public DateTime Now() => DateTime.UtcNow;
    }
}