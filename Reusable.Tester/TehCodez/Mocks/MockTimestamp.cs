using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

namespace Reusable.Utilities.MSTest.Mocks
{
    public class MockTimestamp : LogAttachement
    {
        private readonly int _timestampCount;

        private readonly IEnumerator<DateTime> _timestamps;

        // There is no pretty way to get the name without `1

        public MockTimestamp(IEnumerable<DateTime> timestamps) : base("Timestamp")
        {
            var timestampList = timestamps.ToList();
            _timestamps = timestampList.GetEnumerator();
            _timestampCount = timestampList.Count;
        }

        public override object Compute(ILog log)
        {
            if (_timestamps.MoveNext())
            {
                return _timestamps.Current;
            }

            throw new InvalidOperationException($"You provided only {_timestampCount} timestamps but more were required.");
        }
    }
}