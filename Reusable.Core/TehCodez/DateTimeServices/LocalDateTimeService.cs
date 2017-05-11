using System;

namespace Reusable.DateTimeServices
{
    public class LocalDateTimeService : IDateTimeService
    {
        public DateTime Now() => DateTime.Now;
    }
}
