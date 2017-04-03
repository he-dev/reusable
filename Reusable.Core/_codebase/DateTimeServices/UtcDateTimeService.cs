using System;

namespace Reusable.DateTimeServices
{
    public class UtcDateTimeService : IDateTimeService
    {
        public DateTime Now() => DateTime.UtcNow;    
    }
}