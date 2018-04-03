using System;

namespace Reusable.DateTimes
{
    public class LocalDateTime : IDateTime
    {
        public DateTime Now() => DateTime.Now;
    }
}
