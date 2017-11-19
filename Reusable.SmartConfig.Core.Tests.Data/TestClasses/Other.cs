using System;

namespace Reusable.SmartConfig.Tests.Common.TestClasses
{
    public class Other
    {
        public Boolean Boolean { get; set; }
        public TestEnum Enum { get; set; }
        public DateTime DateTime { get; set; }

        //[Ignore]
        public string IgnoreString { get; set; } = "Ignore value";
    }
}