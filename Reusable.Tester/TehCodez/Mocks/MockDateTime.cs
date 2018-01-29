using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Utilities.MSTest.Mocks
{
    public class MockDateTime : IDateTime
    {
        private readonly IEnumerator<DateTime> _nows;
        private int _counter;

        public MockDateTime(IEnumerable<DateTime> nows)
        {
            if (nows is null)
            {
                throw new ArgumentNullException(nameof(nows));
            }

            _nows = nows.ToList().GetEnumerator();
        }

        public DateTime Now()
        {
            _counter++;
            return
                _nows.MoveNext()
                    ? _nows.Current
                    : throw new InvalidOperationException(
                        $"There {(_counter - 1 == 1 ? "was" : "were")} " +
                        $"only {_counter - 1} {(_counter == 1 ? "timestamp" : "timestamps")} " +
                        $"but more were requested.");
        }
    }
}
