using System;

namespace Reusable.Tester.Mocks
{
    public class MockDateTime : IDateTime
    {
        private readonly Func<DateTime> _now;

        public MockDateTime(Func<DateTime> now) => _now = now ?? throw new ArgumentNullException(nameof(now));

        public DateTime Now() => _now();
    }
}
