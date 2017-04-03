using System;

namespace Reusable.DateTimeServices
{
    public class FixedDateTimeService : IDateTimeService
    {
        private readonly DateTime _now;

        public FixedDateTimeService(DateTime now)
        {
            _now = now;
        }

        DateTime IDateTimeService.Now() => _now;
    }
}
