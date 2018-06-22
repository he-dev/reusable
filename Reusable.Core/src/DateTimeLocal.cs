using System;

namespace Reusable
{
    public class DateTimeLocal : IDateTime
    {
        public DateTime Now() => DateTime.Now;
    }
}
