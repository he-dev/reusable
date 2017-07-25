using System;
using Reusable.Data.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
{
    public class OtherConfiguration
    {
        public Boolean Boolean { get; set; }
        public TestEnum Enum { get; set; }
        public DateTime DateTime { get; set; }

        [Ignore]
        public string IgnoreString { get; set; } = "Ignore value";
    }
}