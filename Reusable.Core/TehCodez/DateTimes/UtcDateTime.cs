using System;

namespace Reusable.DateTimes
{
    public class UtcDateTime : IDateTime
    {
        public DateTime Now() => DateTime.UtcNow;
    }
}