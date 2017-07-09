using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Loggex;

namespace Reusable.Logging.Tests.Mocks
{
    class MockProperty : ComputedProperty
    {
        private readonly object _value;
        public MockProperty(object value) => _value = value;
        public override object Compute(LogEntry log) => _value;
    }
}
